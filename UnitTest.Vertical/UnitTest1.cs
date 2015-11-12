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

        private Contract.contract.AdminstrationContract getAdministrationContract()
        {
            return new AdministratorInterfaceStub();
        }

        [TestMethod]
        public void CustomerContractGetAllTrips()
        {
            List<Contract.dto.Trip> tripList = new List<Contract.dto.Trip>();
            Contract.contract.CustomerContract cis = getCustomerContract();
            // backend says: We dont know if the frontend wants us to thrown any specific
            tripList = cis.GetAllTrips();
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

        [TestMethod]
        public void CustomerContractCreateCustomerReservation()
        {
            Contract.contract.CustomerContract cis = getCustomerContract();
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            // This will be a long list after a few days.
            // A parameter to indicate route and/or date and time?
            List<Contract.dto.Trip> tripList = cis.GetAllTrips();

            Contract.dto.Trip trip = tripList[0];

            //So the CustomerFrontend saves the username and password... That sound extremly safe.. maybe use the email since that should be unique?
            Contract.dto.Customer customer = cis.GetCustomerByLogin("anders", "and");

            double totalPrice = 500.00;
            int numberOfPeople = 2000;

            // And now we use the adminInterface, where Vehicle can be found throught its email?
            Contract.dto.Vehicle vehicle = ais.GetVehicle("anders@and.com");


            Contract.dto.Reservation reservation = null;
            try
            {
                reservation = cis.CreateCustomerReservation(trip, customer, totalPrice, numberOfPeople, vehicle);
            }
            catch (Contract.eto.TripNotFoundException ex)
            {
                Assert.Fail("Trip not found: "  + ex.Message);
            }
            catch(Contract.eto.CustomerNotFoundException ex)
            {
                Assert.Fail("Customer not found: " + ex.Message);
            }
            catch (Contract.eto.VehicleNotFoundException ex)
            {
                Assert.Fail("Vehicle not found: " + ex.Message);
            }

            Assert.IsNotNull(reservation);
        }

        [TestMethod]
        public void CustomerCancelCustomerReservation()
        {
            // This would be so much simpler if we had a GetReservation(int reservationid)
            Contract.contract.CustomerContract cis = getCustomerContract();
            List<Contract.dto.Reservation> resList = null;
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
            if (resList == null)
                Assert.Fail("Could not find a valid reservation for customer.customerid=1");
            else
            {
                if (resList.Count == 0)
                {
                    Assert.Fail("Could not find a valid reservation for customer.customerid=1");
                }
                else
                {
                    int oldCount = resList.Count;
                    cis.CancelCustomerReservation(resList[0]);

                    try
                    {
                        resList = cis.GetAllCustomerReservations(cust);
                    }
                    catch (Contract.eto.CustomerNotFoundException)
                    {
                        Assert.Fail("Invalid customer");
                    }

                    Assert.AreNotEqual(oldCount, resList.Count);
                }
            }
            
        }

    }
}