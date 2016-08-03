using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CityProject
{
    class Program
    {
        static List<City> mData = new List<City>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Command line error!"); Console.Read();
                return;
            }

            City chicagoCity = null;

            Console.WriteLine("Loading cities...");
            using (StreamReader streamReader = new StreamReader(args[0]))
            {
                for (string sLine; (sLine = streamReader.ReadLine()) != null;)
                {
                    City node = City.Load(sLine);
                    if (node != null)
                    {
                        if (node.Name == "Chicago")
                            chicagoCity = node;
                        mData.Add(node);
                    }
                }
            }


            Console.WriteLine("Manipulating Cities_By_Population...");
            mData.Sort(delegate (City city1, City city2) {
                int nCompare = -city1.Population.CompareTo(city2.Population);
                if (nCompare == 0)
                {
                    nCompare = city1.State.CompareTo(city2.State);
                    if (nCompare == 0)
                        nCompare = city1.Name.CompareTo(city2.Name);
                }
                return nCompare;
            });
            string sPath = Path.Combine(Path.GetDirectoryName(args[0]), "Cities_By_Population.txt");
            bool bFirst = true;
            int nPopulation = 0;
            using (StreamWriter streamWriter = new StreamWriter(sPath))
            {
                foreach (City city in mData)
                {
                    string sOutput = "";
                    if (bFirst || city.Population != nPopulation)
                        sOutput += city.Population.ToString() + "\r\n\r\n";
                    bFirst = false;
                    nPopulation = city.Population;
                    sOutput += city.Name + ", " + city.State + "\r\n" + city.GetInterstatesString() + "\r\n\r\n";
                    streamWriter.Write(sOutput);
                }
            }

            Console.WriteLine("Manipulating Interstates_By_City...");
            sPath = Path.Combine(Path.GetDirectoryName(args[0]), "Interstates_By_City.txt");
            using (StreamWriter streamWriter = new StreamWriter(sPath))
            {
                foreach (InterstateInfo interstateInfo in City.InterstateInfos.Values)
                {
                    streamWriter.Write("I-" + interstateInfo.Interstate.ToString() + " " + interstateInfo.CityCount.ToString() + "\r\n");
                }
            }

            if (chicagoCity == null)
            {
                Console.WriteLine("Chicago does not exsit!"); Console.Read();
                return;
            }

            Console.WriteLine("Manipulating Degrees_From_Chicago...");
            List<City> cities = new List<City>();
            chicagoCity.DegreeByChicago = 0;
            cities.Add(chicagoCity);
            Degree(cities);
            mData.Sort(delegate (City city1, City city2) {
                int nCompare = -city1.DegreeByChicago.CompareTo(city2.DegreeByChicago);
                if (nCompare == 0)
                {
                    nCompare = city1.Name.CompareTo(city2.Name);
                    if (nCompare == 0)
                        nCompare = city1.State.CompareTo(city2.State);
                }
                return nCompare;
            });
            sPath = Path.Combine(Path.GetDirectoryName(args[0]), "Degrees_From_Chicago.txt");
            using (StreamWriter streamWriter = new StreamWriter(sPath))
            {
                foreach (City city in mData)
                {
                    streamWriter.Write(city.DegreeByChicago.ToString() + " " + city.Name.ToString() + " " + city.State.ToString() + "\r\n");
                }
            }

            Console.WriteLine("Completed!"); Console.Read();
        }

        static bool Contains(City city, List<City> cities)
        {
            foreach (City n in cities)
            {
                if (city.Name == n.Name && city.State == n.State)
                    return true;
            }
            return false;
        }

        static void Degree(List<City> cities)
        {
            var city = cities.Last();
            foreach (int nInterstate in city.Interstates)
            {
                foreach (City tCity in mData)
                {
                    if (tCity.Interstates.Contains(nInterstate))
                    {
                        if (((uint)tCity.DegreeByChicago) >= cities.Count)
                        {
                            tCity.DegreeByChicago = cities.Count;
                            if (!Contains(tCity, cities))
                            {
                                var tCities = new List<City>(cities);
                                tCities.Add(tCity);
                                Degree(tCities);
                            }
                        }
                    }
                }
            }
        }
    }

    class InterstateInfo
    {
        public int Interstate { get; set; }
        public int CityCount { get; set; }

        public InterstateInfo(int nInterstate, int nCityCount)
        {
            Interstate = nInterstate;
            CityCount = nCityCount;
        }
    }

    class City
    {
        public string Name { get; set; }
        public string State { get; set; }
        public int Population { get; set; }
        public List<int> Interstates { get; private set; }
        public static SortedList<int, InterstateInfo> InterstateInfos = new SortedList<int, InterstateInfo>();
        public int DegreeByChicago { get; set; }

        public City()
        {
            Interstates = new List<int>();
            DegreeByChicago = -1;
        }

        public static City Load(string sLine)
        {
            try
            {
                string[] nodes = sLine.Split(new char[] { '|' });
                if (nodes.Length < 4)
                    return null;
                City city = new City();
                city.Population = Convert.ToInt32(nodes[0]);
                city.Name = nodes[1];
                city.State = nodes[2];
                string[] interstates = nodes[3].Split(new char[] { ';' });
                foreach (string sInterstate in interstates)
                {
                    if (sInterstate.IndexOf("I-") == 0)
                    {
                        int nInterstate = Convert.ToInt32(sInterstate.Substring(2));
                        InterstateInfo interstateInfo = null;
                        if (InterstateInfos.TryGetValue(nInterstate, out interstateInfo))
                            interstateInfo.CityCount++;
                        else
                        {
                            interstateInfo = new InterstateInfo(nInterstate, 1);
                            InterstateInfos.Add(nInterstate, interstateInfo);
                        }
                        city.Interstates.Add(nInterstate);
                    }
                }
                city.Interstates.Sort();
                return city;
            }
            catch (Exception) { }
            return null;
        }

        public string GetInterstatesString()
        {
            bool bFirst = true;
            string sOutput = "Interstates: ";
            foreach (int nInterstate in Interstates)
            {
                sOutput += (bFirst ? "I-" : ", I-") + nInterstate.ToString();
                bFirst = false;
            }
            return sOutput;
        }
    }
}
