using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTest.Vertical
{
    [TestClass]
    public class UnitTest1
    {
        private Contract.contract.CustomerContract getCustomerContract()
        {
            return new CustomerInterfaceStub();
        }

        [TestMethod]
        public void CustomerContractGetAllTrips()
        {
            List<Contract.dto.Trip> tripList = new List<Contract.dto.Trip>();
            Contract.contract.CustomerContract cis = getCustomerContract();
            try
            {
                tripList = cis.GetAllTrips();
            }
            catch (Exception ex) // backend says: We dont know if the frontend wants us to thrown any specific
            {
                Assert.Fail(ex.Message);
            }

            Assert.AreNotEqual(0, tripList.Count);
        }

        [TestMethod]
        public void CustomerContractGetAllCustomerReservations()
        {
            List<Contract.dto.Reservation> resList = new List<Contract.dto.Reservation>();
            Contract.contract.CustomerContract cis = getCustomerContract();
            Contract.dto.Customer cust = new Contract.dto.Customer()
            {
                CustomerId = 1
            };
            try
            {
                resList = cis.GetAllCustomerReservations(cust);
            }
            catch (Contract.eto.CustomerNotFoundException)
            {
                Assert.Fail("Invalid customer");
            }
            Assert.AreNotEqual(0, resList);
        }

        [TestMethod]
        public void CustomerContractCreateCustomer()
        {
            Contract.contract.CustomerContract cis = getCustomerContract();
            Contract.dto.Customer cust = new Contract.dto.Customer()
            {
                CustomerId = 1
            };
            //backend: No special exception. 
            bool result = cis.CreateCustomer(cust);
            Assert.AreEqual(true, result);
        }


    }
}