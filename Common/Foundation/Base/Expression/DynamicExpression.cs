using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class DynamicExpression 
    {
        /// an expression is a path to a field/property/method in a class, starting from the origin object.
        /// Evaluate<T>() returns the object the expression points to, using reflection.
        /// examples :
        /// "MyMethod()" will return the method "MyMethod" in the origin's type class
        /// "myMember" will return the "myMember" field/property in the origin's type class
        /// "myMember.innerMember.innerMethod()" will return the method "innerMethod" inside "innerMember", inside "MyMember"
        /// etc.
        ///
        /// fields/propertie/methods can be public or private
        public static T Evaluate<T>(object origin, string expression)
        {   
            try
            {
                // Split the expression into parts separated by '.'
                string[] parts = expression.Split('.');

                // Start with the origin object
                object currentObject = origin;

                foreach (string part in parts)
                {
                    // Check if the part indicates a method invocation
                    if (part.EndsWith("()"))
                    {
                        // Extract method name by removing "()" suffix
                        string methodName = part.Substring(0, part.Length - 2);

                        // Get the method info from the current object's type
                        var methodInfo = currentObject.GetType().GetMethod(methodName,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                        // Ensure the method exists
                        if (methodInfo == null)
                            throw new MissingMethodException($"Method {methodName} does not exist on type {currentObject.GetType().Name}");

                        // Invoke the method and update the currentObject
                        currentObject = methodInfo.Invoke(currentObject, null);
                    }
                    else
                    {
                        // Otherwise, treat it as a field or property
                        var fieldInfo = currentObject.GetType().GetField(part,
                            BindingFlags.Public   | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                        if (fieldInfo != null)
                        {
                            // If it's a field, get the value
                            currentObject = fieldInfo.GetValue(currentObject);
                        }
                        else
                        {
                            // Check for a property
                            var propertyInfo = currentObject.GetType().GetProperty(part,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                            // Ensure the property exists
                            if (propertyInfo == null)
                                throw new MissingMemberException($"Field or property {part} does not exist on type {currentObject.GetType().Name}");

                            // If it's a property, get the value
                            currentObject = propertyInfo.GetValue(currentObject);
                        }
                    }
                }

                // Attempt to cast the final result to the expected type
                return (T)currentObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error evaluating expression '{expression}': {ex.Message}");
                throw;
            }
        }
        
        public static void SetValue(object origin, string expression, object value)
        {
            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////// Test
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Test Expression Evaluation")]
        #endif
        private static void TestExpressionEvaluation()
        {
            // Create a test object
            var testObject = new TestClass();

            // Test different expressions
            Debug.Log($"Field1 Result               : {Evaluate<int>   (testObject, "Field1")}"               );
            Debug.Log($"TestMethod() Result         : {Evaluate<int>   (testObject, "TestMethod()")}"         );
            Debug.Log($"Nested.InnerField Result    : {Evaluate<string>(testObject, "Nested.InnerField")}"    );
            Debug.Log($"Nested.InnerMethod() Result : {Evaluate<string>(testObject, "Nested.InnerMethod()")}" );
        }
        
        public class TestClass
        {
            public int        Field1         = 42;
            public int        TestMethod()   => 100;
            public InnerClass Nested         = new();
        }
        public class InnerClass
        {
            public string InnerField     = "Hello World";
            public string InnerMethod()  => "Inner method invoked!";
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////
        
    }
}

