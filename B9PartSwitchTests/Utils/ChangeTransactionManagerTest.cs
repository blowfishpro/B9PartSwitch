using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using B9PartSwitch.Utils;

namespace B9PartSwitchTests.Utils
{
    public class ChangeTransactionManagerTest
    {
        [Fact]
        public void TestConstructor__Null()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ChangeTransactionManager(null);
            });
        }

        [Fact]
        public void TestRequestChange__NotYetInitialized()
        {
            int counter = 0;
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => counter += 1);
            transactionManager.RequestChange();
            Assert.Equal(0, counter);
        }

        [Fact]
        public void TestRequestChange__OutsideTransaction()
        {
            int counter = 0;
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => counter += 1);
            transactionManager.Initialize();
            transactionManager.RequestChange();
            Assert.Equal(1, counter);
        }

        [Fact]
        public void TestWithTransaction__NoRequestChange()
        {
            int counter = 0;
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => counter += 1);
            transactionManager.WithTransaction(delegate
            {
                Assert.Equal(0, counter);
            });

            Assert.Equal(0, counter);
        }

        [Fact]
        public void TestWithTransaction__RequestChange()
        {
            int counter = 0;
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => counter += 1);
            transactionManager.WithTransaction(delegate
            {
                transactionManager.RequestChange();
                transactionManager.RequestChange();
                Assert.Equal(0, counter);
            });

            Assert.Equal(1, counter);
        }

        [Fact]
        public void TestWithTransaction__RequestChange__AlreadyChanging()
        {
            ChangeTransactionManager transactionManager = null;
            transactionManager = new ChangeTransactionManager(() => transactionManager.RequestChange());
            Assert.Throws<InvalidOperationException>(delegate
            {
                transactionManager.WithTransaction(delegate
                {
                    transactionManager.RequestChange();
                });
            });
        }

        [Fact]
        public void TestWithTransaction__Throws()
        {
            int counter = 0;
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => counter += 1);
            Exception ex1 = new Exception();
            Exception ex2 = Assert.Throws<Exception>(delegate
            {
                transactionManager.WithTransaction(delegate
                {
                    throw ex1;
                });
            });

            Assert.Equal(0, counter);
            Assert.Same(ex1, ex2);

            transactionManager.RequestChange();
            Assert.Equal(1, counter);
        }

        [Fact]
        public void TestWithTransaction__Null()
        {
            ChangeTransactionManager transactionManager = new ChangeTransactionManager(() => throw new Exception());
            Assert.Throws<ArgumentNullException>(delegate
            {
                transactionManager.WithTransaction(null);
            });
        }
    }
}
