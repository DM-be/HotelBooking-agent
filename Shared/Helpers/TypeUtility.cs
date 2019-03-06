using System;
using System.Reflection;
using System.Reflection.Emit;

namespace HotelBot.Shared.Helpers
{

    //https://www.codeproject.com/Articles/14560/Fast-Dynamic-Property-Field-Accessors
    public class TypeUtility<ObjectType>
    {
        public delegate MemberType
               MemberGetDelegate<MemberType>(ObjectType obj);

        public static MemberGetDelegate<MemberType>
            GetMemberGetDelegate<MemberType>(string memberName)
        {
            Type objectType = typeof(ObjectType);

            PropertyInfo pi = objectType.GetProperty(memberName);
            FieldInfo fi = objectType.GetField(memberName);
            if (pi != null)
            {
                // Member is a Property...

                MethodInfo mi = pi.GetGetMethod();
                if (mi != null)
                {
                    // NOTE:  As reader J. Dunlap pointed out...
                    //  Calling a property's get accessor is faster/cleaner using
                    //  Delegate.CreateDelegate rather than Reflection.Emit 
                    return (MemberGetDelegate<MemberType>)
                        Delegate.CreateDelegate(typeof(
                              MemberGetDelegate<MemberType>), mi);
                }
                else
                    throw new Exception(String.Format(
                        "Property: '{0}' of Type: '{1}' does" +
                        " not have a Public Get accessor",
                        memberName, objectType.Name));
            }
            else if (fi != null)
            {
                // Member is a Field...

                DynamicMethod dm = new DynamicMethod("Get" + memberName,
                    typeof(MemberType), new Type[] { objectType }, objectType);
                ILGenerator il = dm.GetILGenerator();
                // Load the instance of the object (argument 0) onto the stack
                il.Emit(OpCodes.Ldarg_0);
                // Load the value of the object's field (fi) onto the stack
                il.Emit(OpCodes.Ldfld, fi);
                // return the value on the top of the stack
                il.Emit(OpCodes.Ret);

                return (MemberGetDelegate<MemberType>)
                    dm.CreateDelegate(typeof(MemberGetDelegate<MemberType>));
            }
            else
                throw new Exception(String.Format(
                    "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
                    memberName, objectType.Name));
        }
    }
}
