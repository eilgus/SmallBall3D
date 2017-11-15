using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace XFramework
{
    //既是泛型 又是类模板       因为要实现复用所以模板 因为要对应不同类型的复用所以用了泛型
    public abstract class XSingleton<T> where T : XSingleton<T> 
    {
        protected static T instance = null;

        protected XSingleton()
        {

        }

        public static T GetInstance()
        {
            //寻找非公共的构造方法 
            if (instance == null)
            {
                ConstructorInfo[] ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                ConstructorInfo ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                if(ctor == null)
                    throw new Exception("Non-public ctor() not found!");
                instance = ctor.Invoke(null) as T;
            }

            return instance;
        }

    }
}
