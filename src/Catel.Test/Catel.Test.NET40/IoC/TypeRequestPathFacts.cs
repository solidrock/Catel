﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeRequestPathFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Test.IoC
{
    using System;
    using Catel.IoC;

#if NETFX_CORE
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    public class TypeRequestPathFacts
    {
        public class X
        {
            public X(Z z) { }
        }

        public class Y
        {
            public Y(X x) { }
        }

        public class Z
        {
            public Z(Y y) { }
        }

        private static TypeRequestInfo[] CreateArrayWithOnlyReferenceTypes()
        {
            return new TypeRequestInfo[]
            {
                new TypeRequestInfo(typeof(X)),
                new TypeRequestInfo(typeof(Y)), 
                new TypeRequestInfo(typeof(Z))
            };
        }

        private static TypeRequestInfo[] CreateArrayWithOnlyValueTypes()
        {
            return new TypeRequestInfo[]
            {
                new TypeRequestInfo(typeof(int)),
                new TypeRequestInfo(typeof(double)), 
                new TypeRequestInfo(typeof(DateTime))
            };
        }

        private static TypeRequestInfo[] CreateMixedArray()
        {
            return new TypeRequestInfo[]
            {
                new TypeRequestInfo(typeof(X)),
                new TypeRequestInfo(typeof(Y)), 
                new TypeRequestInfo(typeof(Z))
            };
        }

        private static TypeRequestInfo[] CreateInvalidPath()
        {
            return new TypeRequestInfo[]
            {
                new TypeRequestInfo(typeof(X)),
                new TypeRequestInfo(typeof(Y)), 
                new TypeRequestInfo(typeof(Z)),
                new TypeRequestInfo(typeof(X))
            };
        }

        [TestClass]
        public class TheConstructor
        {
            [TestMethod]
            public void ThrowsArgumentNullExceptionForNullAndEmptyTypeRequestInfos()
            {
                ExceptionTester.CallMethodAndExpectException<ArgumentException>(() => new TypeRequestPath((TypeRequestInfo)null));
                ExceptionTester.CallMethodAndExpectException<ArgumentException>(() => new TypeRequestPath((TypeRequestInfo[])null));
                ExceptionTester.CallMethodAndExpectException<ArgumentException>(() => new TypeRequestPath(new TypeRequestInfo[] { }));
            }

            [TestMethod]
            public void AddsTypeRequestInfosWithValueTypes()
            {
                var typeArray = CreateArrayWithOnlyValueTypes();

                var path = new TypeRequestPath(typeArray);

                for (int i = 0; i < typeArray.Length; i++)
                {
                    Assert.AreEqual(typeArray[i], path.AllTypes[i]);
                }
                    
                Assert.AreEqual(typeArray[0], path.FirstType);
                Assert.AreEqual(typeArray[typeArray.Length - 1], path.LastType);
            }

            [TestMethod]
            public void AddsTypeRequestInfosWithReferenceTypes()
            {
                var typeArray = CreateArrayWithOnlyReferenceTypes();

                var path = new TypeRequestPath(typeArray);

                for (int i = 0; i < typeArray.Length; i++)
                {
                    Assert.AreEqual(typeArray[i], path.AllTypes[i]);
                }

                Assert.AreEqual(typeArray[0], path.FirstType);
                Assert.AreEqual(typeArray[typeArray.Length - 1], path.LastType);
            }

            [TestMethod]
            public void AddsTypeRequestInfosWithMixedTypes()
            {
                var typeArray = CreateMixedArray();

                var path = new TypeRequestPath(typeArray);

                for (int i = 0; i < typeArray.Length; i++)
                {
                    Assert.AreEqual(typeArray[i], path.AllTypes[i]);
                }

                Assert.AreEqual(typeArray[0], path.FirstType);
                Assert.AreEqual(typeArray[typeArray.Length - 1], path.LastType);
            }
        }

        [TestClass]
        public class TheFirstTypeProperty
        {
            [TestMethod]
            public void ReturnsRightType()
            {
                var typeArray = CreateArrayWithOnlyValueTypes();
                var path = new TypeRequestPath(typeArray);

                Assert.AreEqual(typeArray[0], path.FirstType);
            }
        }

        [TestClass]
        public class TheLastTypeProperty
        {
            [TestMethod]
            public void ReturnsRightType()
            {
                var typeArray = CreateArrayWithOnlyValueTypes();
                var path = new TypeRequestPath(typeArray);

                Assert.AreEqual(typeArray[typeArray.Length - 1], path.LastType);
            }
        }

        [TestClass]
        public class TheIsValidProperty
        {
            [TestMethod]
            public void ReturnsTrueForValidPath()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);
             
                Assert.IsTrue(path.IsValid);
            }

            [TestMethod]
            public void ReturnsFalseForInvalidPath()
            {
                var typeArray = CreateInvalidPath();
                var path = new TypeRequestPath(typeArray);

                Assert.IsFalse(path.IsValid);
            }
        }

        [TestClass]
        public class ThePushTypeMethod
        {
            [TestMethod]
            public void ThrowsArgumentNullExceptionForNullTypeRequestInfo()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);

                ExceptionTester.CallMethodAndExpectException<ArgumentNullException>(() => path.PushType(null, false));
            }

            [TestMethod]
            public void DoesThrowCircularDependencyExceptionForDuplicateTypeIfSpecified()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);

                ExceptionTester.CallMethodAndExpectException<CircularDependencyException>(() => path.PushType(new TypeRequestInfo(typeof(X)), true));
            }

            [TestMethod]
            public void DoesNotThrowCircularDependencyExceptionForDuplicateTypeIfNotSpecified()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);

                path.PushType(new TypeRequestInfo(typeof(X)), false);

                Assert.IsFalse(path.IsValid);
            }

            [TestMethod]
            public void DoesIgnoreValueTypesIfIgnoreValueTypesIsTrue()
            {
                var firstType = new TypeRequestInfo(typeof(X));
                var path = new TypeRequestPath(firstType);

                path.PushType(new TypeRequestInfo(typeof(DateTime)), false);
                Assert.AreEqual(firstType, path.LastType);

                path.PushType(new TypeRequestInfo(typeof(double)), false);
                Assert.AreEqual(firstType, path.LastType);

                path.PushType(new TypeRequestInfo(typeof(int)), false);
                Assert.AreEqual(firstType, path.LastType);
            }

            [TestMethod]
            public void DoesNotIgnoreValueTypesIfIgnoreValueTypesIsFalse()
            {
                var firstType = new TypeRequestInfo(typeof(X));
                var path = new TypeRequestPath(firstType, false);

                path.PushType(new TypeRequestInfo(typeof(DateTime)), false);
                Assert.AreEqual(typeof(DateTime), path.LastType.Type);

                path.PushType(new TypeRequestInfo(typeof(double)), false);
                Assert.AreEqual(typeof(double), path.LastType.Type);

                path.PushType(new TypeRequestInfo(typeof(int)), false);
                Assert.AreEqual(typeof(int), path.LastType.Type);
            }
        }

        [TestClass]
        public class ThePopTypeMethod
        {
            [TestMethod]
            public void ThrowsInvalidOperationExceptionWhenTypeRequestPathOnlyContainsOneType()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);

                while (path.AllTypes.Length > 1)
                {
                    path.PopType();
                }

                ExceptionTester.CallMethodAndExpectException<InvalidOperationException>(path.PopType);
            }
        }

        [TestClass]
        public class TheMarkTypeAsNotCreatedMethod
        {
            [TestMethod]
            public void ThrowsArgumentNullExceptionForNullTypeRequestInfo()
            {
                var typeArray = CreateMixedArray();
                var path = new TypeRequestPath(typeArray);

                ExceptionTester.CallMethodAndExpectException<ArgumentNullException>(() => path.MarkTypeAsNotCreated(null));
            }

            [TestMethod]
            public void RemovesTypeRangeUntilSpecificType()
            {
                var path = new TypeRequestPath(new TypeRequestInfo(typeof(X)));
                path.PushType(new TypeRequestInfo(typeof(object)), false);
                path.PushType(new TypeRequestInfo(typeof(Y)), false);
                path.PushType(new TypeRequestInfo(typeof(object)), false);
                path.PushType(new TypeRequestInfo(typeof(Z)), false);

                path.MarkTypeAsNotCreated(new TypeRequestInfo(typeof(Y)));

                Assert.AreEqual(2, path.TypeCount);
                Assert.AreEqual(new TypeRequestInfo(typeof(X)), path.AllTypes[0]);
                Assert.AreEqual(new TypeRequestInfo(typeof(object)), path.AllTypes[1]);
            }
        }
    }
}