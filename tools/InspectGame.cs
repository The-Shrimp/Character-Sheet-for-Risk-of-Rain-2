using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

// Reads metadata/IL only. Never loads game assemblies for execution.
class InspectGame
{
    static string root;
    static Dictionary<short, OpCode> codes;
    static int Main(string[] args)
    {
        root = args[0];
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, e) => {
            string file = Path.Combine(root, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(file) ? Assembly.ReflectionOnlyLoadFrom(file) : Assembly.ReflectionOnlyLoad(e.Name);
        };
        var assembly = Assembly.ReflectionOnlyLoadFrom(Path.Combine(root, "RoR2.dll"));
        codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)).ToDictionary(o => o.Value);
        if (args.Length < 2 || args[1] == "--find") {
            Type[] discovered; try { discovered = assembly.GetTypes(); } catch (ReflectionTypeLoadException ex) { discovered = ex.Types.Where(t => t != null).ToArray(); foreach (var failure in ex.LoaderExceptions.Select(e => e.Message).Distinct()) Console.WriteLine("LOAD WARNING " + failure); }
            foreach (var type in discovered.Where(t => (args.Length > 2 ? t.FullName.IndexOf(args[2], StringComparison.OrdinalIgnoreCase)>=0 : t.FullName.IndexOf("Pause", StringComparison.OrdinalIgnoreCase)>=0) || t.FullName.IndexOf("Scoreboard",StringComparison.OrdinalIgnoreCase)>=0 || t.FullName.IndexOf("MPEventSystem",StringComparison.OrdinalIgnoreCase)>=0)) Console.WriteLine(type.FullName);
            return 0;
        }
        var target = assembly.GetType(args[1], true);
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        Console.WriteLine("TYPE " + target.FullName + " : " + target.BaseType);
        foreach (var f in target.GetFields(flags)) try { Console.WriteLine("FIELD " + (f.IsPublic ? "public " : "nonpublic ") + f); } catch (Exception ex) { Console.WriteLine("FIELD " + f.Name + " [unresolved: " + ex.Message + "]"); }
        foreach (var p in target.GetProperties(flags)) try { Console.WriteLine("PROPERTY " + p); } catch (Exception ex) { Console.WriteLine("PROPERTY " + p.Name + " [unresolved: " + ex.Message + "]"); }
        foreach (var e in target.GetEvents(flags)) Console.WriteLine("EVENT " + e);
        foreach (var m in target.GetMethods(flags)) try { Console.WriteLine("METHOD " + (m.IsPublic ? "public " : "nonpublic ") + m); } catch (Exception ex) { Console.WriteLine("METHOD " + m.Name + " [unresolved: " + ex.Message + "]"); }
        if (args.Length > 2) foreach (var method in target.GetMethods(flags).Where(m => args.Skip(2).Contains(m.Name))) Dump(method);
        return 0;
    }
    static void Dump(MethodInfo method)
    {
        Console.WriteLine("IL " + method);
        var body = method.GetMethodBody();
        if (body == null) return;
        var il = body.GetILAsByteArray();
        for (int i=0; i<il.Length;) {
            int offset=i;
            short value=il[i++];
            if(value==0xfe) value=(short)(0xfe00 | il[i++]);
            OpCode code=codes[value];
            object operand="";
            switch(code.OperandType) {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineI: operand=(sbyte)il[i++]; break;
                case OperandType.ShortInlineVar: operand=il[i++]; break;
                case OperandType.InlineVar: operand=BitConverter.ToUInt16(il,i); i+=2; break;
                case OperandType.ShortInlineBrTarget: operand=i+1+(sbyte)il[i]; i++; break;
                case OperandType.InlineBrTarget: operand=i+4+BitConverter.ToInt32(il,i); i+=4; break;
                case OperandType.InlineI: operand=BitConverter.ToInt32(il,i); i+=4; break;
                case OperandType.InlineI8: operand=BitConverter.ToInt64(il,i); i+=8; break;
                case OperandType.ShortInlineR: operand=BitConverter.ToSingle(il,i); i+=4; break;
                case OperandType.InlineR: operand=BitConverter.ToDouble(il,i); i+=8; break;
                case OperandType.InlineSwitch: int n=BitConverter.ToInt32(il,i); i+=4; operand="switch count="+n; i+=n*4; break;
                default:
                    int token=BitConverter.ToInt32(il,i); i+=4;
                    try { operand=code.OperandType==OperandType.InlineString ? method.Module.ResolveString(token) : (object)method.Module.ResolveMember(token,method.DeclaringType.GetGenericArguments(),method.GetGenericArguments()); }
                    catch { operand="token="+token.ToString("x8"); }
                    break;
            }
            Console.WriteLine(offset.ToString("x4") + " " + code.Name + " " + operand);
        }
    }
}



