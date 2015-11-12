using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTest.Vertical
{
    /// <summary>
    /// Note: Backend made this test, making educated guesses reguarding usage of exceptions.
    /// XML documentation missing from Contract project
    /// </summary>
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

        #region Tests for Contract.contract.CustomerContract

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
            bool result = false;
            try
            {
                result = cis.CreateCustomer(cust);
            }
            catch (Contract.eto.CustomerNotFoundException)
            {
                Assert.Fail("CustomerNotFoundException");
            }
            Assert.IsTrue(result);
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
                Assert.Fail("Trip not found: " + ex.Message);
            }
            catch (Contract.eto.CustomerNotFoundException ex)
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
        #endregion

        #region Tests for Contract.contract.AdminstrationContract

        /*
            Backend: Not sure what to test in the Create-methods. Should there be a difference between the parameter and the return value?
            Should we return an object so the frontend gets the id?

            And why do all Get-methods have email as parameter? 
        */

        [TestMethod]
        public void AdminstrationContractCreateCustomer()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Customer newCustomer = new Contract.dto.Customer()
            {
                CustomerId = 1,
                Mail = "anders@and.com"
            };

            // Backend question: What do we do if this fails? return null or some random exception?
            Contract.dto.Customer returnValue = ais.CreateCustomer(newCustomer);

            Assert.AreEqual<Contract.dto.Customer>(newCustomer, returnValue);
        }

        [TestMethod]
        public void AdminstrationContractGetCustomer()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Customer customer = null;
            try
            {
                customer = ais.GetCustomer("anders@and.com");
            }
            catch (Contract.eto.CustomerNotFoundException ex)
            {
                Assert.Fail("CustomerNotFoundException: " + ex.Message);
            }
            Assert.IsNotNull(customer);
        }

        [TestMethod]
        public void AdminstrationContractUpdateCustomer()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Customer customer = ais.GetCustomer("anders@and.com");
            int oldValue = customer.AmountOfFreeRides;
            customer.AmountOfFreeRides = customer.AmountOfFreeRides + 1;
            try
            {
                customer = ais.UpdateCustomer(customer);
            }
            catch (Contract.eto.CustomerNotFoundException ex)
            {
                Assert.Fail("CustomerNotFoundException: " + ex.Message);
            }

            Assert.AreEqual(oldValue + 1, customer.AmountOfFreeRides);
        }

        [TestMethod]
        public void AdminstrationContractDeleteCustomer()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Customer customer = ais.GetCustomer("anders@and.com");
            try
            {
                ais.DeleteCustomer(customer);
            }
            catch (Contract.eto.CustomerNotFoundException ex)
            {
                Assert.Fail("CustomerNotFoundException: " + ex.Message);
            }
            try
            {
                customer = ais.GetCustomer("anders@and.com");
            }
            catch (Contract.eto.CustomerNotFoundException ex)
            {
                customer = null;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.IsNull(customer);
        }

        [TestMethod]
        public void AdminstrationContractCreateFerry()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Ferry newFerry = new Contract.dto.Ferry()
            {
                DockId = 1, // What is this? A secret class?
                FerryId = 1,
                Municipality = "Andeby",
                Name = "Hugo",
                PassengerCapacity = 7,
                Size = "VERY BIG"
            };
            // Backend: Since we have no exceptions in eto to throw, we just return null if something went wrong?
            Contract.dto.Ferry returnValue = ais.CreateFerry(newFerry);

            Assert.AreEqual<Contract.dto.Ferry>(newFerry, returnValue);
        }

        [TestMethod]
        public void AdminstrationContractGetFerry()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();


            Contract.dto.Ferry newFerry = null;
            try
            {
                //Backend: So a ferry have an email? 
                //Its missing in the DTO... 
                //so we cant save it. 
                //you can create millions of ferrys, but GetFerry will always fail...
                newFerry = ais.GetFerry("anders@and.com");
            }
            catch (Contract.eto.FerryNotFoundException ex)
            {
                Assert.Fail("FerryNotFoundException: " + ex.Message);
            }
            Assert.IsNotNull(newFerry);
        }

        [TestMethod]
        public void AdminstrationContractUpdateFerry()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();


            Contract.dto.Ferry newFerry = null;
            try
            {
                //Backend: So a ferry have an email? 
                //Its missing in the DTO... 
                //so we cant save it. 
                //you can create millions of ferrys, but GetFerry will always fail...
                newFerry = ais.GetFerry("anders@and.com");
            }
            catch (Contract.eto.FerryNotFoundException ex)
            {
                Assert.Fail("FerryNotFoundException: " + ex.Message);
            }
            int oldValue = newFerry.PassengerCapacity;
            newFerry.PassengerCapacity++;
            Contract.dto.Ferry returnValue = ais.UpdateFerry(newFerry);

            Assert.AreEqual(oldValue + 1, returnValue.PassengerCapacity);
        }

        [TestMethod]
        public void AdminstrationContractDeleteFerry()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Ferry newFerry = ais.GetFerry("anders@and.com");
            bool result = false;
            try
            {
                result = ais.DeleteFerry(newFerry);
            }
            catch (Contract.eto.FerryNotFoundException)
            {
                Assert.Fail("FerryNotFoundException");
            }
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void AdminstrationContractCreateReservation()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Reservation reservation = new Contract.dto.Reservation()
            {
                TripId = 1,
                CustomerId = 1,
                NumberOfPeople = 900,
                ReservationId = 43,
                TotalPrice = 500000.47,
                VehicleId = 12
            };

            Contract.dto.Reservation resultReservation = null;
            try
            {
                resultReservation = ais.CreateReservation(reservation);
            }
            catch (Contract.eto.CustomerNotFoundException)
            {
                Assert.Fail("CustomerNotFoundException");
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }

            Assert.AreEqual<Contract.dto.Reservation>(reservation, resultReservation);
        }

        [TestMethod]
        public void AdminstrationContractGetReservation()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Reservation reservation = null;
            try
            {
                reservation = ais.GetReservation("anders@and.com");
            }
            catch (Contract.eto.ReservationNotFoundException)
            {
                Assert.Fail("ReservationNotFoundException");
            }

            Assert.IsNotNull(reservation);

        }

        [TestMethod]
        public void AdminstrationContractUpdateReservation()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Reservation reservation = null;
            try
            {
                reservation = ais.GetReservation("anders@and.com");
            }
            catch (Contract.eto.ReservationNotFoundException)
            {
                Assert.Fail("ReservationNotFoundException");
            }
            reservation.TotalPrice++;
            Contract.dto.Reservation resultReservation = null;
            try
            {
                resultReservation = ais.UpdateReservation(reservation);
            }
            catch (Contract.eto.ReservationNotFoundException)
            {
                Assert.Fail("ReservationNotFoundException");
            }
            Assert.AreEqual(reservation.TotalPrice, resultReservation.TotalPrice);
        }

        [TestMethod]
        public void AdminstrationContractDeleteReservation()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Reservation reservation = null;
            try
            {
                reservation = ais.GetReservation("anders@and.com");
            }
            catch (Contract.eto.ReservationNotFoundException)
            {
                Assert.Fail("ReservationNotFoundException");
            }

            bool result = false;
            try
            {
                result = ais.DeleteReservation(reservation);
            }
            catch (Contract.eto.ReservationNotFoundException)
            {
                Assert.Fail("ReservationNotFoundException");
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminstrationContractCreateTrip()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Trip trip = new Contract.dto.Trip()
            {
                FerryId = 1,
                Price = 2.25,
                RouteId = 1,
                TripId = 1
            };

            Contract.dto.Trip returnTrip = null;
            try
            {
                returnTrip = ais.CreateTrip(trip);
            }
            catch (Contract.eto.FerryNotFoundException)
            {
                Assert.Fail("FerryNotFoundException");
            }
            catch (Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }

            Assert.AreEqual<Contract.dto.Trip>(trip, returnTrip);
        }

        [TestMethod]
        public void AdminstrationContractGetTrip()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Trip trip = null;
            try
            {
                trip = ais.GetTrip("anders@and.com");
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }
            Assert.IsNotNull(trip);
        }

        [TestMethod]
        public void AdminstrationContractUpdateTrip()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Trip trip = null;
            try
            {
                trip = ais.GetTrip("anders@and.com");
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }

            trip.Price++;
            Contract.dto.Trip returnTrip = null;
            try
            {
                returnTrip = ais.UpdateTrip(trip);
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }

            Assert.AreEqual<Contract.dto.Trip>(trip, returnTrip);
        }

        [TestMethod]
        public void AdminstrationContractDeleteTrip()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Trip trip = null;
            try
            {
                trip = ais.GetTrip("anders@and.com");
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }
            bool result = false;
            try
            {
                result = ais.DeleteTrip(trip);
            }
            catch (Contract.eto.TripNotFoundException)
            {
                Assert.Fail("TripNotFoundException");
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminstrationContractCreateVehicle()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();

            Contract.dto.Vehicle vehicle = new Contract.dto.Vehicle()
            {
                VehicleId = 1,
                VehicleSize = 1,
                VehicleType = "M109"
            };

            Contract.dto.Vehicle returnVehicle = null;
            returnVehicle = ais.CreateVehicle(vehicle);

            Assert.AreEqual<Contract.dto.Vehicle>(vehicle, returnVehicle);

        }

        [TestMethod]
        public void AdminstrationContractGetVehicle()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Vehicle item = null;
            try
            {
                item = ais.GetVehicle("anders@and.com");
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void AdminstrationContractUpdateVehicle()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Vehicle item = null;
            try
            {
                item = ais.GetVehicle("anders@and.com");
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }

            item.VehicleSize++;
            Contract.dto.Vehicle returnItem = null;
            try
            {
                returnItem = ais.UpdateVehicle(item);
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }

            Assert.AreEqual(item.VehicleSize, returnItem.VehicleSize);
        }

        [TestMethod]
        public void AdminstrationContractDeleteVehicle()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Vehicle item = null;
            try
            {
                item = ais.GetVehicle("anders@and.com");
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }
            bool returnValue = false;
            try
            {
                returnValue = ais.DeleteVehicle(item);
            }
            catch (Contract.eto.VehicleNotFoundException)
            {
                Assert.Fail("VehicleNotFoundException");
            }
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        public void AdminstrationContractCreateRoute()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            // So where do we Depart FROM?
            Contract.dto.Route item = new Contract.dto.Route()
            {
                Depature = DateTime.Now,
                Destination = "Somewhere else than here",
                Duration = 900
            };

            Contract.dto.Route returnItem = null;
            returnItem = ais.CreateRoute(item);

            Assert.AreEqual<Contract.dto.Route>(item, returnItem);
        }

        [TestMethod]
        public void AdminstrationContractGetRoute()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Route item = null;
            try
            {
                item = ais.GetRoute("anders@and.com");
            }
            catch (Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void AdminstrationContractUpdateRoute()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Route item = null;
            try
            {
                item = ais.GetRoute("anders@and.com");
            }
            catch (Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }

            item.Duration++;
            Contract.dto.Route returnItem = null;
            try
            {
                returnItem = ais.UpdateRoute(item);
            }
            catch (Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }

            Assert.AreEqual(item.Duration, returnItem.Duration);
        }

        [TestMethod]
        public void AdminstrationContractDeleteRoute()
        {
            Contract.contract.AdminstrationContract ais = getAdministrationContract();
            Contract.dto.Route item = null;
            try
            {
                item = ais.GetRoute("anders@and.com");
            }
            catch (Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }

            bool result = false;
            try
            {
                result = ais.DeleteRoute(item);
            }
            catch(Contract.eto.RouteNotFoundException)
            {
                Assert.Fail("RouteNotFoundException");
            }
            Assert.IsTrue(result); 
        }

        [TestMethod]
        public void AdminstrationContractGetCustomers()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }

        [TestMethod]
        public void AdminstrationContractGetFerries()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }

        public void AdminstrationContractGetTrips()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }

        [TestMethod]
        public void AdminstrationContractGetReservations()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }

        public void AdminstrationContractGetVehicles()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }

        [TestMethod]
        public void AdminstrationContractGetRoutes()
        {
            Assert.Fail("Not in interface, but described in documentation");
        }
        #endregion
    }
}