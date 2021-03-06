﻿using System;
using System.Collections.Generic;

namespace TeamBeastMode
{
    
    //Function SolveByCar(),Function SolveByCarBonus(),SolveByComplete(),SolveSimple(),SolveByTime(),SolveByCarTime();

    abstract class Solver
    {
        private int Rows;
        private int Columns;

        public List<Vehicle> Vehicles { get; private set; }
        public int Bonus { get; private set; }
        protected int Steps;

        protected List<Ride> Rides;

        public void Load(string fileName)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(' ');
                int rows = int.Parse(parts[0]);
                int columns = int.Parse(parts[1]);
                int vehiclesCount = int.Parse(parts[2]);
                int ridesCount = int.Parse(parts[3]);
                int bonus = int.Parse(parts[4]);
                int steps = int.Parse(parts[5]);

                List<Vehicle> vehicles = new List<Vehicle>();
                for (int i = 0; i < vehiclesCount; i++)
                    vehicles.Add(new Vehicle(i));

                List<Ride> rides = new List<Ride>();
                for (int i = 0; i < ridesCount; i++)
                {
                    line = sr.ReadLine();
                    parts = line.Split(' ');
                    int startR = int.Parse(parts[0]);
                    int startC = int.Parse(parts[1]);
                    int endR = int.Parse(parts[2]);
                    int endC = int.Parse(parts[3]);
                    int timeStart = int.Parse(parts[4]);
                    int timeEnd = int.Parse(parts[5]);

                    Ride ride = new Ride(i, startR, startC, endR, endC, timeStart, timeEnd);

                    rides.Add(ride);
                }

                this.Rows = rows;
                this.Columns = columns;

                this.Vehicles = vehicles;
                this.Rides = rides;

                this.Bonus = bonus;
                this.Steps = steps;
            }
        }

        public int CalculateScore()
        {
            int totalScore = 0;

            foreach (Vehicle vehicle in this.Vehicles)
                totalScore += vehicle.DriveDistance + vehicle.BonusCollected * this.Bonus;

            return totalScore;
        }

        public void WriteOutput(string fileName)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName))
            {
                foreach (Vehicle vehicle in this.Vehicles)
                {
                    List<int> ridesAssigned = vehicle.RidesAssigned;
                    sw.Write(ridesAssigned.Count);
                    for (int i = 0; i < ridesAssigned.Count; i++)
                    {
                        sw.Write(" ");
                        sw.Write(ridesAssigned[i]);
                    }
                    sw.WriteLine();
                }
            }
        }

        public int CalcMaxPossibleScore()
        {
            int totalDistance = 0;
            foreach (Ride ride in Rides)
                totalDistance += ride.Distance;

            return totalDistance + Rides.Count * Bonus;
        }

        public abstract void Solve();
    }

    class SolverByCarTime : Solver
    {
        public override void Solve()
        {
            Rides.Sort(new Ride.CompareByStartTime());
            Rides.Reverse();
            Vehicle.CompareByTimeDriveEnd carSortTimeDriveEnd = new Vehicle.CompareByTimeDriveEnd();
            Vehicles.Sort(carSortTimeDriveEnd);
            int currentTime = -1;

            while (Rides.Count > 0)
            {
                Vehicles.Sort(carSortTimeDriveEnd);

                currentTime++;
                if (Vehicles[0].TimeDriveEnd > currentTime)
                    currentTime = Vehicles[0].TimeDriveEnd;

                if (currentTime >= this.Steps)
                    break;

                // Clean rides list
                for (int ridePos = Rides.Count - 1; ridePos >= 0; ridePos--)
                {
                    Ride ride = Rides[ridePos];

                    // If ride not possible
                    if (ride.TimeLatestStart < currentTime)
                    {
                        Rides.RemoveAt(ridePos);
                        continue;
                    }
                }

                for (int i = 0; i < Vehicles.Count; i++)
                {
                    Vehicle car = Vehicles[i];

                    // This car (and all after) can't do rides in time
                    if (car.TimeDriveEnd > currentTime)
                        break;

                    Ride bestRide;
                    int bestStartTime;
                    FindBestRideForCarMaxTime(car, currentTime, out bestRide, out bestStartTime);

                    if (bestRide == null)
                        continue;

                    // Check if valid
                    // Complete ride on time
                    int rideStartTime = Math.Max(bestRide.TimeStart, bestStartTime);
                    if (rideStartTime + bestRide.Distance >= bestRide.TimeEnd)
                        continue;

                    if (rideStartTime + bestRide.Distance >= Steps)
                        continue;

                    // Add ride to car
                    car.AddRide(bestRide, bestRide.EndR, bestRide.EndC, rideStartTime + bestRide.Distance);
                    Rides.Remove(bestRide);
                }
            }
        }

        private void FindBestRideForCarMaxTime(Vehicle car, int maxTimeToStart, out Ride bestRide, out int bestStartTime)
        {
            bestRide = null;
            bestStartTime = 0;
            int bestTimeToDrive = 0;
            double bestScoreDensity = 0;

            for (int ridePos = Rides.Count - 1; ridePos >= 0; ridePos--)
            {
                Ride ride = Rides[ridePos];
                if (ride.TimeStart > maxTimeToStart)
                    break;

                int timeToDrive = car.TimeToPosition(ride.StartR, ride.StartC);
                int carToStart = car.TimeDriveEnd + timeToDrive;
                if (carToStart > maxTimeToStart)
                    continue;
                if (carToStart >= ride.TimeEnd)
                    continue;

                int startTime = Math.Max(carToStart, ride.TimeStart);
                if (startTime + ride.Distance >= ride.TimeEnd)
                    continue;

                int bonus = (startTime == ride.TimeStart) ? Bonus : 0;
                double scoreDensity = (double)(ride.Distance + bonus) / (double)(startTime + ride.Distance - car.TimeDriveEnd);

                if (bestRide == null)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }
                else if (ride.Distance < bestRide.Distance)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }

                /*
                else if (timeToDrive < bestTimeToDrive)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }
                */
                /*else if (startTime < bestStartTime)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }*/
            }
        }
    }

    class SolverByRideTime : Solver
    {
        public override void Solve()
        {
            Rides.Sort(new Ride.CompareByStartTime());
            Rides.Reverse();
            Vehicle.CompareByTimeDriveEnd carSortTimeDriveEnd = new Vehicle.CompareByTimeDriveEnd();
            Vehicles.Sort(carSortTimeDriveEnd);
            int currentTime = -1;

            while (Rides.Count > 0)
            {
                currentTime++;
                if (Vehicles[0].TimeDriveEnd > currentTime)
                    currentTime = Vehicles[0].TimeDriveEnd;

                if (currentTime >= this.Steps)
                    break;

                for (int ridePos = Rides.Count - 1; ridePos >= 0; ridePos--)
                {
                    Ride ride = Rides[ridePos];

                    // When too far in list
                    if (ride.TimeStart > currentTime)
                        break;

                    // If ride not possible
                    if (ride.TimeLatestStart < currentTime)
                    {
                        Rides.RemoveAt(ridePos);
                        continue;
                    }

                    Vehicle bestCar;
                    int bestStartTime;
                    FindBestCarForRideMaxTime(ride, currentTime, out bestCar, out bestStartTime);

                    if (bestCar == null)
                        continue;

                    // Check if valid
                    // Complete ride on time
                    int rideStartTime = Math.Max(ride.TimeStart, bestStartTime);
                    if (rideStartTime + ride.Distance >= ride.TimeEnd)
                        continue;

                    if (rideStartTime + ride.Distance >= Steps)
                        continue;

                    // Add ride to car
                    bestCar.AddRide(ride, ride.EndR, ride.EndC, rideStartTime + ride.Distance);
                    Rides.RemoveAt(ridePos);

                    // Optimization - if not vehicle is free at this time - not need to continue checking
                    Vehicles.Sort(carSortTimeDriveEnd);
                    if (Vehicles[0].TimeDriveEnd > currentTime)
                        break;
                }
            }
        }

        private void FindBestCarForRideMaxTime(Ride ride, int maxTimeToStart, out Vehicle bestCar, out int bestStartTime)
        {
            bestCar = null;
            bestStartTime = 0;

            foreach (Vehicle car in Vehicles)
            {
                if (car.TimeDriveEnd > maxTimeToStart)
                    break;

                int carToStart = car.TimeDriveEnd + car.TimeToPosition(ride.StartR, ride.StartC);
                if (carToStart > maxTimeToStart)
                    continue;

                if (carToStart + ride.Distance >= ride.TimeEnd)
                    continue;

                if (bestCar == null)
                {
                    bestCar = car;
                    bestStartTime = carToStart;
                }
                else if (carToStart < bestStartTime)
                {
                    bestCar = car;
                    bestStartTime = carToStart;
                }
            }
        }
    }

    class SolverByCarBonus : Solver
    {
        public override void Solve()
        {
            for (int carPos = 0; carPos < Vehicles.Count; carPos++)
            {
                Vehicle car = Vehicles[carPos];
                while (true)
                {
                    Ride bestRide;
                    int bestStartTime;

                    FindBestRideForCarBonus(car, out bestRide, out bestStartTime);

                    if (bestRide != null)
                    {
                        car.AddRide(bestRide, bestRide.EndR, bestRide.EndC, bestStartTime + bestRide.Distance);
                        // Remove ride from list
                        for (int i = 0; i < Rides.Count; i++)
                            if (Rides[i].ID == bestRide.ID)
                            {
                                Rides.RemoveAt(i);
                                break;
                            }
                    }
                    else
                        break;
                }
            }

        }

        private void FindBestRideForCarBonus(Vehicle car, out Ride bestRide, out int bestStartTime)
        {
            bestRide = null;
            bestStartTime = 0;
            int bestHasBonus = 0;

            foreach (Ride ride in Rides)
            {
                int hasBonus;
                int carToStart = car.TimeDriveEnd + car.TimeToPosition(ride.StartR, ride.StartC);
                if (carToStart >= ride.TimeEnd)
                    continue;
                int startTime = Math.Max(carToStart, ride.TimeStart);
                if (startTime + ride.Distance >= ride.TimeEnd)
                    continue;

                hasBonus = (carToStart <= ride.TimeStart) ? 50 * Bonus : 0;
                if (bestRide == null)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestHasBonus = hasBonus;
                }
                else if (startTime - hasBonus < bestStartTime - bestHasBonus)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestHasBonus = hasBonus;
                }
            }
        }
    }

    class SolverByCar : Solver
    {
        public override void Solve()
        {
            for (int carPos = 0; carPos < Vehicles.Count; carPos++)
            {
                Vehicle car = Vehicles[carPos];
                while (true)
                {
                    Ride bestRide;
                    int bestStartTime;

                    FindBestRideForCar(car, out bestRide, out bestStartTime);

                    if (bestRide != null)
                    {
                        car.AddRide(bestRide, bestRide.EndR, bestRide.EndC, bestStartTime + bestRide.Distance);
                        // Remove ride from list
                        for (int i = 0; i < Rides.Count; i++)
                            if (Rides[i].ID == bestRide.ID)
                            {
                                Rides.RemoveAt(i);
                                break;
                            }
                    }
                    else
                        break;
                }
            }
        }

        private void FindBestRideForCar(Vehicle car, out Ride bestRide, out int bestStartTime)
        {
            bestRide = null;
            bestStartTime = 0;
            int bestTimeToDrive = 0;
            double bestScoreDensity = 0;

            foreach (Ride ride in Rides)
            {
                int timeToDrive = car.TimeToPosition(ride.StartR, ride.StartC);
                int carToStart = car.TimeDriveEnd + timeToDrive;
                if (carToStart >= ride.TimeEnd)
                    continue;
                int startTime = Math.Max(carToStart, ride.TimeStart);
                if (startTime + ride.Distance >= ride.TimeEnd)
                    continue;

                int bonus = (startTime == ride.TimeStart) ? Bonus : 0;
                double scoreDensity = (double)(ride.Distance + bonus) / (double)(startTime + ride.Distance - car.TimeDriveEnd);

                if (bestRide == null)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }
               
                /*// Doesn't improve on best score (improve on problem 'e')
                else if (scoreDensity > bestScoreDensity)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }
                 * */
                
                else if (startTime < bestStartTime)
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                    bestScoreDensity = scoreDensity;
                }
                /*
                // Doesn't improve anything
                else if ((startTime == bestStartTime) && (timeToDrive < bestTimeToDrive))
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                    bestTimeToDrive = timeToDrive;
                }
                */
                /*
                // Doesn't improve anything
                else if ((startTime == bestStartTime) && (bestRide.Distance < ride.Distance))
                {
                    bestRide = ride;
                    bestStartTime = startTime;
                }
                */
            }
        }
    }

    class SolverByRideComplete : Solver
    {
        public override void Solve()
        {
            Rides.Sort(new Ride.CompareByStartTime());
            Vehicle.CompareByTimeDriveEnd carSortTimeDriveEnd = new Vehicle.CompareByTimeDriveEnd();

            while (Rides.Count > 0)
            {
                Vehicles.Sort(carSortTimeDriveEnd);

                Vehicle bestGlobalCar = null;
                int bestGlobalCompleteTime = int.MaxValue;
                Ride bestGlobalRide = null;

                foreach (Ride ride in Rides)
                {
                    // All other rides will start later
                    if (ride.TimeStart > bestGlobalCompleteTime)
                        break;

                    int rideTime = ride.Distance;

                    // Find fastest car for the ride
                    Vehicle bestCar;
                    int bestCompleteTime;
                    FindBestCarForRideComplete(ride, out bestCar, out bestCompleteTime);

                    // Check if valid
                    // Complete ride on time
                    if (bestCompleteTime >= ride.TimeEnd)
                        continue;

                    if (bestCompleteTime >= Steps)
                        continue;

                    if (bestGlobalCar == null)
                    {
                        bestGlobalCar = bestCar;
                        bestGlobalCompleteTime = bestCompleteTime;
                        bestGlobalRide = ride;
                    }
                    else if (bestCompleteTime < bestGlobalCompleteTime)
                    {
                        bestGlobalCar = bestCar;
                        bestGlobalCompleteTime = bestCompleteTime;
                        bestGlobalRide = ride;
                    }
                }

                if (bestGlobalRide == null)
                    return;

                // Add ride to car
                bestGlobalCar.AddRide(bestGlobalRide,
                    bestGlobalRide.EndR, bestGlobalRide.EndC,
                    bestGlobalCompleteTime);

                // Remove ride from list
                for (int i = 0; i < Rides.Count; i++)
                    if (Rides[i].ID == bestGlobalRide.ID)
                    {
                        Rides.RemoveAt(i);
                        break;
                    }
            }
        }

        private void FindBestCarForRideComplete(Ride ride, out Vehicle bestCar, out int bestCompleteTime)
        {
            bestCar = null;
            bestCompleteTime = 0;
            int bestDriveTime = 0;

            foreach (Vehicle car in Vehicles)
            {
                int carDriveTime = car.TimeToPosition(ride.StartR, ride.StartC);
                int carToStart = car.TimeDriveEnd + carDriveTime;
                int completeTime = Math.Max(carToStart, ride.TimeStart) + ride.Distance;

                if (bestCar == null)
                {
                    bestCar = car;
                    bestCompleteTime = completeTime;
                    bestDriveTime = carDriveTime;
                }
                else if (completeTime < bestCompleteTime)
                {
                    bestCar = car;
                    bestCompleteTime = completeTime;
                    bestDriveTime = carDriveTime;
                }
                // Optional optimization - works in some cases
                else if ((completeTime == bestCompleteTime) && (carDriveTime < bestDriveTime))
                {
                    bestCar = car;
                    bestCompleteTime = completeTime;
                    bestDriveTime = carDriveTime;
                }
            }
        }
    }

    class SolverByRide : Solver
    {
        public override void Solve()
        {
            Rides.Sort(new Ride.CompareByStartTime());
            Vehicle.CompareByTimeDriveEnd carSortTimeDriveEnd = new Vehicle.CompareByTimeDriveEnd();

            while (Rides.Count > 0)
            {
                Ride ride = Rides[0];

                Rides.RemoveAt(0);

                // Find fastest car for the ride
                Vehicles.Sort(carSortTimeDriveEnd);
                Vehicle bestCar;
                int bestStartTime;
                FindBestCarForRide(ride, out bestCar, out bestStartTime);

                if (bestCar == null)
                    continue;

                // Check if valid
                // Complete ride on time
                int rideStartTime = Math.Max(ride.TimeStart, bestStartTime);
                if (rideStartTime + ride.Distance >= ride.TimeEnd)
                    continue;

                if (rideStartTime + ride.Distance >= Steps)
                    continue;

                // Add ride to car
                bestCar.AddRide(ride, ride.EndR, ride.EndC, rideStartTime + ride.Distance);
            }
        }

        private void FindBestCarForRide(Ride ride, out Vehicle bestCar, out int bestStartTime)
        {
            bestCar = null;
            bestStartTime = 0;

            foreach (Vehicle car in Vehicles)
            {
                int carToStart = car.TimeDriveEnd + car.TimeToPosition(ride.StartR, ride.StartC);
                if (carToStart + ride.Distance >= ride.TimeEnd)
                    continue;

                if (bestCar == null)
                {
                    bestCar = car;
                    bestStartTime = carToStart;
                }
                else if (carToStart < bestStartTime)
                {
                    bestCar = car;
                    bestStartTime = carToStart;
                }
            }
        }
    }
}
