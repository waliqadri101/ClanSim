using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClanSim
{
    internal class Program
    {
        #region functionality to move the console application to a desired x and y, for easy debugging
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// this method is used to move the console application to x and y location. this functionality is not cross platform
        /// and only specific to windows.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void MoveConsoleApplication(int x, int y)
        {
            SetWindowPos(GetConsoleWindow(), IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        #endregion

        static void Main(string[] args)
        {
            // move the application to the given coordinates on the screen
            Program.MoveConsoleApplication(1000, 0);


            PopulationManager oPopulationManager = new PopulationManager();
            oPopulationManager.Run();
            Console.ReadLine();
        }
    }

    /// <summary>
    /// This class simulates population dynamics over a period of time.
    /// </summary>
    /// <remarks>
    /// <para><strong>Population Initialization:</strong></para>
    /// <para>Initializes a population of 100 people with random ages and genders. The simulation iterates over this population for 100 years.</para>
    ///
    /// <para><strong>Marriages:</strong></para>
    /// <para>People get married based on eligibility criteria, checked in the <c>GettingMarried</c> method. Marriages are only between opposite sexes (as per the comment, no same-sex marriages are allowed).</para>
    ///
    /// <para><strong>Aging and Death:</strong></para>
    /// <para>Each year, everyone’s age increases by one, and people are checked for death based on their age. When a person dies, their spouse’s <c>CurrentSpouseId</c> is reset to 0.</para>
    ///
    /// <para><strong>Summary:</strong></para>
    /// <para>A summary of the population's status is printed each year, including counts of men, women, married people, and deaths.</para>
    /// </remarks>
    public class PopulationManager
    {
        // constants
        public static readonly int INITIAL_POPULATION_SIZE = 6;
        public static readonly int NO_OF_YEARS = 1000;

        // population list with persons
        public List<Person> Population = new List<Person>();

        /// <summary>
        /// Runs the simulation
        /// </summary>
        public void Run()
        {
            InitializePopulation();

            for (int currentYear = 1; currentYear <= NO_OF_YEARS; currentYear++)
            {
                Helper.msg($"The year is {currentYear}", ConsoleColor.Yellow);
                Helper.msg($"===============", ConsoleColor.Yellow);

                Summary();

                GettingMarried();

                StartFamily();

                HappyBirthdayToAll();

                // if the population is wipedout then display the summary and end simulation
                if (IsPopulationWipedOut())
                {
                    Helper.msg($"Everyone is dead, ending simulation".ToUpper(), ConsoleColor.Blue);
                    Summary();
                    break;
                }

                Thread.Sleep(100);
            }
        }

        private bool IsPopulationWipedOut()
        {
            return !Population.Where(p => p.IsAlive).Any();
        }

        private void StartFamily()
        {
            Helper.msg($"Initialiing StartFamily Method on this batch", ConsoleColor.Blue);

            int populationCount = this.Population.Count;
            for (int i = 0; i < populationCount; i++)
            {
                var person = this.Population[i];
                var spouse = GetSpouse(person);

                TryReproduce(person, spouse);
            }
        }

        /// <summary>
        /// This method will take person and their spouse, and try to reproduce
        /// </summary>
        /// <param name="person">Person</param>
        /// <param name="spouse">Person's spouse</param>
        private void TryReproduce(Person person, Person spouse)
        {
            if (person.IsAlive &&
                person.IsMarried &&
                person.IsFertile)
            {
                if (spouse.IsAlive &&
                    spouse.IsMarried &&
                    spouse.IsFertile)
                {
                    // check if each parent is able to have kids, even if one parent can have kids then they will have kids.
                    if (person.Childrens.Count < Person._NO_OF_CHILDS ||
                        spouse.Childrens.Count < Person._NO_OF_CHILDS)
                    {
                        Random random = new Random();

                        // get the spouse

                        // get the next available id
                        int newId = getNextAvailableId();

                        // get random age for 10 to 25
                        int babyAge = 0;

                        // generate a random Gender for this person
                        Person.SexType randomGenderForThisPerson;
                        if (random.Next(0, 2) == 0)
                        {
                            randomGenderForThisPerson = Person.SexType.Male;
                        }
                        else
                        {
                            randomGenderForThisPerson = Person.SexType.Female;
                        }

                        Person baby = new Person
                        {
                            Id = newId,
                            Name = $"Person {newId}",
                            Age = babyAge,
                            Gender = randomGenderForThisPerson
                        };

                        Helper.msg($"{Person.PersonInfoString(person)} & {Person.PersonInfoString(spouse)} is having baby {Person.PersonInfoString(baby)}", ConsoleColor.Magenta);

                        // save the information in objects and the population list
                        person.Childrens.Add(baby);
                        spouse.Childrens.Add(baby);
                        this.Population.Add(baby);
                    }
                }
            }
        }

        /// <summary>
        /// Get next available int id in the population
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private int getNextAvailableId()
        {
            int maxId = Population.Max(person => person.Id);
            maxId++;

            if (DoesIdExist(maxId))
            {
                throw new Exception("DoesIdExist() : Id already exits");
            }
            else
            {
                return maxId;
            }
        }

        /// <summary>
        /// Checks if the id already exists in the population
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool DoesIdExist( int id)
        {
            return Population.Any(person => person.Id == id);
        }

        /// <summary>
        /// This method will print a little summary of the population. like how many people how many men / women
        /// Married/Non-Married Death toll. Childrens etc.
        /// </summary>
        private void Summary()
        {
            int totalPopulation = Population.Where(person => person.IsAlive).Count();
            int noOfWomen = Population.Where(person => person.Gender == Person.SexType.Female && person.IsAlive).Count();
            int noOfMen = Population.Where(person => person.Gender == Person.SexType.Male && person.IsAlive).Count();
            int marriedWomen = Population.Where(person => person.Gender == Person.SexType.Female && person.IsMarried == true && person.IsAlive).Count();
            int marriedMen = Population.Where(person => person.Gender == Person.SexType.Male && person.IsMarried == true && person.IsAlive).Count();
            int menWhoDied = Population.Where(person => person.Gender == Person.SexType.Male && !person.IsAlive).Count();
            int womenWhoDied = Population.Where(person => person.Gender == Person.SexType.Female && !person.IsAlive).Count();
            int totalUnMarriedPeople = Population.Where(person => !person.IsMarried && person.IsAlive).Count();

            Helper.msg("Summary", ConsoleColor.Blue);
            Helper.msg("=======", ConsoleColor.Blue);


            Helper.msg($"--> Total population {totalPopulation}", ConsoleColor.Yellow);

            if ((menWhoDied + womenWhoDied) > 0)
                Helper.msg($"--> Total dead {menWhoDied + womenWhoDied}", ConsoleColor.Red);

            Helper.msg($"--> Total No Of Men {noOfMen}", ConsoleColor.Yellow);

            Helper.msg($"--> Total No Of Women {noOfWomen}", ConsoleColor.Yellow);

            Helper.msg($"--> Total Married Men {marriedMen}", ConsoleColor.Yellow);

            Helper.msg($"--> Total Married Women {marriedWomen}", ConsoleColor.Yellow);

            if (totalUnMarriedPeople > 0)
                Helper.msg($"--> Total Un-Married People {totalUnMarriedPeople}", ConsoleColor.Yellow);

            if (menWhoDied > 0)
                Helper.msg($"--> Total Men who Died {menWhoDied}", ConsoleColor.Red);

            if (womenWhoDied > 0)
                Helper.msg($"--> Total Women who Died {womenWhoDied}", ConsoleColor.Red);

            #region SNAPSHOT THE POPULATION HERE
            foreach (var person in Population)
            {
                Helper.msg($"----> {Person.PersonInfoString(person)}", ConsoleColor.DarkGreen, false);
                var personSpouse = GetSpouse(person);

                if (personSpouse != null && personSpouse.Id > 0)
                {
                    Helper.msg($" is married to  {Person.PersonInfoString(personSpouse)}", ConsoleColor.Yellow, withNewLine: false);
                }
                else
                {
                    Helper.msg("", withNewLine: false);
                }

                if (!person.IsAlive)
                {
                    Helper.msg(" (Deceased)", ConsoleColor.Red, withNewLine: true);
                }
                else
                {
                    Helper.msg("", ConsoleColor.Red, withNewLine: true);
                }
            }
            #endregion
        }

        /// <summary>
        /// This initializes the Population with INITIAL_POPULATION_SIZE persons. 
        /// </summary>
        private void InitializePopulation()
        {
            this.Population = new();
            Random random = new Random();

            for (int i = 1; i <= PopulationManager.INITIAL_POPULATION_SIZE; i++)
            {
                // get random age for 10 to 25
                int randomAgeForThisPerson = random.Next(1, 26);

                // generate a random Gender for this person

                Person.SexType randomGenderForThisPerson;
                if (random.Next(0, 2) == 0)
                {
                    randomGenderForThisPerson = Person.SexType.Male;
                }
                else
                {
                    randomGenderForThisPerson = Person.SexType.Female;
                }

                Person eachPerson = new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    Age = randomAgeForThisPerson,
                    Gender = randomGenderForThisPerson
                };

                this.Population.Add(eachPerson);
            }
        }

        /// <summary>
        /// Who ever is alive in the Population list will get 1 year incremented to their age.
        /// </summary>
        private void HappyBirthdayToAll()
        {
            for (int i = 0; i < this.Population.Count; i++)
            {
                var eachPerson = this.Population[i];

                if (eachPerson.IsAlive)
                {   // increment the age to the people who are alive.
                    eachPerson.Age++;

                    if (!eachPerson.IsAlive)
                    {   // if anyone dies after the new age
                        RemoveSpouse(eachPerson);
                    }
                }
            }

        }

        /// <summary>
        /// This method will get each person from the population list, and try to marry them with someone.
        /// who is CanMarry(), CanMarry method checks for if the person is have opposite sex, is alive, of age, not previously married.
        /// sorry no gay marriages yet. :)
        /// </summary>
        /// <param name="population"></param>
        private void GettingMarried()
        {
            Helper.msg($"Initialiing GettingMarried Method on this batch", ConsoleColor.Blue);
            for (int i = 0; i < this.Population.Count; i++)
            {
                var eachPerson = this.Population[i];

                if (eachPerson.IsAlive && !eachPerson.IsMarried && eachPerson.CanMarry())
                {

                    // FindMater method finds the mate and also marrys them together
                    bool isMarriedNow = FindMate(this.Population, eachPerson);
                    if (!isMarriedNow)
                    {
                        Helper.msg($"-> {Person.PersonInfoString(eachPerson)} Can't find mate.");
                    }
                }
            }
        }

        /// <summary>
        /// This method iterate thru the population and see who is available to be married with the Person person.
        /// </summary>
        /// <param name="person">The Person instance that is the candidate</param>
        /// <returns>return true if person gets married to someone. </returns>
        private bool FindMateAndMarry(Person person)
        {
            for (int i = 0; i < Population.Count; i++)
            {
                var significantOther = Population[i];

                if (significantOther.Gender != person.Gender &&
                    significantOther.CanMarry() &&
                    significantOther.Id != person.Id &&
                    significantOther.IsAlive)
                {

                    Helper.msg($"-> {Person.PersonInfoString(person)} got married to {Person.PersonInfoString(significantOther)})");

                    significantOther.SetSpouse(person.Id);
                    significantOther.Spouses.Add(person);

                    person.SetSpouse(significantOther.Id);
                    person.Spouses.Add(significantOther);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the spouse from the person in the case of death of the person or devorce etc.
        /// This will not delete the spouse from the Spouses list of that person. this will only change CurrentSpouseId to 0
        /// on both sides. means person.CurrentSpouseId and significantOther.CurrentSpouseId will saved to 0
        /// </summary>
        /// <param name="person">Person object</param>
        private void RemoveSpouse(Person person)
        {
            // remove the spouse id from the couple, a dead person can't stay married. and significant other is also no longer married.
            if (person.CurrentSpouseId > 0)
            {
                var significantOther = this.Population.Where(eachPerson => eachPerson.CurrentSpouseId == person.Id).FirstOrDefault();

                Helper.msg($"{Person.PersonInfoString(person)} was married to {Person.PersonInfoString(significantOther!)}", ConsoleColor.Red);

                significantOther!.CurrentSpouseId = 0;
                person.CurrentSpouseId = 0;
            }
        }

        private Person GetSpouse(Person person)
        {
            // remove the spouse id from the couple, a dead person can't stay married. and significant other is also no longer married.
            if (person.CurrentSpouseId > 0)
            {
                var significantOther = this.Population.Where(eachPerson => eachPerson.CurrentSpouseId == person.Id).FirstOrDefault();
                return significantOther;
            }
            return null;
        }

    }

    public class Person
    {
        // constants
        public static readonly int _DEATH_AGE = 80;
        public static readonly int _FERTILITY_BEGIN_AGE = 16;
        public static readonly int _MARRAGE_AGE = 18;
        public static readonly int _FERTILITY_END_AGE = 65;
        public static readonly int _NO_OF_CHILDS = 2;

        // fields
        private int _age;

        // properties
        public int Id { get; set; }

        public int Age
        {
            get { return _age; }
            set
            {
                if (value == _age)
                    return;

                _age = value;

                if (!IsAlive)
                {
                    Helper.msg($"{Name} just passed away", ConsoleColor.Red);
                }
            }
        }

        public SexType Gender { get; set; }

        public string Name { get; set; }

        public bool IsMarried
        {
            get
            {
                if (this.IsAlive && this.CurrentSpouseId > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int CurrentSpouseId { get; set; } = 0;

        public bool IsAlive
        {
            get
            {
                if (this.Age >= _DEATH_AGE)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsFertile
        {
            get
            {
                if (this.Age >= _FERTILITY_BEGIN_AGE && this.Age <= _FERTILITY_END_AGE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<Person> Spouses { get; set; } = new List<Person>();
        public List<Person> Childrens { get; set; } = new List<Person>();

        // Methods
        public bool CanMarry()
        {
            if (this.Age >= _MARRAGE_AGE && this.IsAlive && this.CurrentSpouseId == 0)
                return true;
            else
                return false;
        }
        public bool SetSpouse(int spouseId)
        {
            this.CurrentSpouseId = spouseId;
            return true;
        }

        // Enum
        public enum SexType
        {
            Male,
            Female
        }

        // static methods
        public static string PersonInfoString(Person person)
        {
            return $"{person.Name} ({person.Age}, {person.Gender})";
        }
    }
    public class Helper
    {
        public static void DisplayArray(int[] arr)
        {
            msg("[", ConsoleColor.DarkYellow, false);
            for (int i = 0; i < arr.Length; i++)
            {
                msg(arr[i].ToString(), ConsoleColor.DarkYellow, false);
                if (i < arr.Length - 1)
                {
                    msg(", ", ConsoleColor.DarkYellow, false);
                }
            }
            msg("]", ConsoleColor.DarkYellow, true);
        }
        public static void msg(string sMessage)
        {
            try
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(sMessage);
                Console.ForegroundColor = originalColor;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.ResetColor();
            }
        }
        public static void msg(string sMessage, ConsoleColor foregroundColor = ConsoleColor.White, bool withNewLine = true)
        {
            try
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
                if (withNewLine)
                {
                    Console.WriteLine(sMessage);
                }
                else
                {
                    Console.Write(sMessage);
                }
                Console.ForegroundColor = originalColor;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.ResetColor();
            }
        }
        public static void PrintError(string sMessage)
        {
            try
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(sMessage);
                Console.ForegroundColor = originalColor;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.ResetColor();
            }
        }
    }

}
