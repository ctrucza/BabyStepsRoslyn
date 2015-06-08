using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleSolution
{
    namespace NestedNamespace
    {
        namespace DoublyNestedNamespace
        {
            namespace NamespaceThreeDeep
            {
                class VeryDeepClass
                {
                    class VeryDeepInnerClass
                    {
                        class MariannaTrench
                        {
                            
                        }
                    }
                }        
            }
        
        }

        class ClassInNestedNamespace
        {
            
        }

        namespace AnotherDoublyNestedNamespace
        {
        
        }
    }

    namespace AnotherNestedNamespace
    {
        class ClassInAnotherNestedNamespace
        {
            
        }
    }

    class OuterClass
    {
        class InnerClass
        {
            class InnerClassInInnerClass
            {
                class ClassThreeDeep
                {
                    
                }
            }
        }

        class AnotherInnerClass
        {
            
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
