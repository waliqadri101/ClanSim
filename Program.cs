// MIT License
// 
// Copyright (c) [2024] [waliqadri101@gmail.com]
// 
// Permission is hereby granted to use, copy, modify, and distribute this software 
// for any purpose with or without fee, provided the above copyright notice appears 
// in all copies.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER LIABILITY, ARISING FROM, OUT OF, 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClanSim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // initialize the popuplation manager
            using PopulationManager oPopulationManager = new PopulationManager();

            // call the run method with parameters, the parameters are optionional
            // if no parameters are provided the default values will be used
            oPopulationManager.Run();

            Console.ReadKey();
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
    public class PopulationManager : IDisposable
    {
        // simulation settings variables, person level settings are in the Person class
        public static int INITIAL_POPULATION_SIZE = 25;
        public static int NO_OF_YEARS = 1000; // no of years the simulation will run for
        public static int PAUSE_BETWEEN_EACH_YEAR = 100;    // pause in milli seconds between each year
        public static bool SHOW_INDIVISUAL_DETAIL = true;   // if you want to show the information about indivisual person.

        // constants
        public readonly string[] commonMaleNames = new string[] { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua", "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas", "Eric", "Stephen", "Jonathan", "Larry", "Justin", "Scott", "Brandon", "Benjamin", "Samuel", "Frank", "Gregory", "Raymond", "Alexander", "Patrick", "Jack", "Dennis", "Jerry", "Tyler", "Aaron", "Jose", "Henry", "Adam", "Douglas", "Nathan", "Peter", "Zachary", "Kyle", "Walter", "Harold", "Jeremy", "Ethan", "Carl", "Keith", "Roger", "Gerald", "Christian", "Terry", "Sean", "Arthur", "Austin", "Noah", "Lawrence", "Jesse", "Joe", "Bryan", "Billy", "Jordan", "Albert", "Dylan", "Bruce", "Willie", "Gabriel", "Alan", "Juan", "Wayne", "Roy", "Ralph", "Randy", "Eugene", "Carlos", "Russell", "Louis", "Philip", "Vincent", "Bobby", "Johnny", "Logan" };
        public readonly string[] commonFemaleNames = new string[] { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen", "Nancy", "Margaret", "Lisa", "Betty", "Dorothy", "Sandra", "Ashley", "Kimberly", "Donna", "Emily", "Michelle", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Laura", "Sharon", "Cynthia", "Kathleen", "Helen", "Amy", "Shirley", "Angela", "Anna", "Brenda", "Pamela", "Nicole", "Ruth", "Katherine", "Samantha", "Christine", "Emma", "Catherine", "Debra", "Virginia", "Rachel", "Carolyn", "Janet", "Maria", "Heather", "Diane", "Julie", "Joyce", "Victoria", "Kelly", "Christina", "Lauren", "Joan", "Evelyn", "Olivia", "Judith", "Megan", "Cheryl", "Martha", "Andrea", "Frances", "Hannah", "Jacqueline", "Ann", "Gloria", "Jean", "Kathryn", "Alice", "Teresa", "Sara", "Janice", "Doris", "Madison", "Julia", "Grace", "Judy", "Abigail", "Marie", "Denise", "Beverly", "Amber", "Theresa", "Marilyn", "Danielle", "Diana", "Brittany", "Natalie", "Sophia", "Rose", "Isabella", "Alexis", "Kayla", "Lillian" };
        
        /// <summary>
        /// population list with persons
        /// </summary>
        public List<Person>? Population = new List<Person>();

        /// <summary>
        /// Runs the simulation
        /// </summary>
        public void Run()
        {
            InitBanner();

            // initialize the population
            InitializePopulation();

            for (int currentYear = 1; currentYear <= NO_OF_YEARS; currentYear++)
            {
                Helper.msg("");
                Helper.msg($"Current year is {currentYear}", ConsoleColor.Gray);
                Helper.msg($"===============", ConsoleColor.Gray);

                Summary();

                GettingMarried();

                StartFamily();

                HappyBirthdayToAll();

                // if the population is wipedout then display the summary and end simulation
                if (IsPopulationWipedOut())
                {
                    Helper.msg($"EVERYONE DIED", ConsoleColor.Blue);
                    Summary();
                    break;
                }

                Thread.Sleep(PAUSE_BETWEEN_EACH_YEAR);
            }
            Helper.msg("SIMULATION ENDED", ConsoleColor.Blue);
        }

        private void InitBanner()
        {

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"*************************************************************");
            Console.WriteLine(@"*                                                           *");
            Console.WriteLine(@"*                   Welcome to ClanSim                      *");
            Console.WriteLine(@"*                                                           *");
            Console.WriteLine(@"*************************************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"");
            Console.WriteLine(@" ██████╗██╗      █████╗ ███╗   ██╗███████╗██╗███╗  ███╗");
            Console.WriteLine(@"██╔════╝██║     ██╔══██╗████╗  ██║██╔════╝╚═╝████╗████║");
            Console.WriteLine(@"██║     ██║     ███████║██╔██╗ ██║███████╗██╗██║███║██║");
            Console.WriteLine(@"██║     ██║     ██╔══██║██║╚██╗██║     ██║██║██║ ╚═╝██║");
            Console.WriteLine(@" ██████╗███████╗██║  ██║██║ ╚████║███████║██║██║    ██║");
            Console.WriteLine(@" ╚═════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚═╝╚═╝    ╚═╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"");
            Console.WriteLine(@"This program simulates the human life cycle, including births,");
            Console.WriteLine(@"marriages, and deaths over the years. Watch as your population");
            Console.WriteLine(@"grows, thrives, and eventually fades away, generation by generation.");
            Console.WriteLine(@"");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"You can tweak the behavior of the simulation, such as:");
            Console.WriteLine(@"- Initial Population Size");
            Console.WriteLine(@"- Number of Children");
            Console.WriteLine(@"- Death Age");
            Console.WriteLine(@"- Number of Years to Run");
            Console.WriteLine(@"- You can also change the speed of the simulation.");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"");
            Console.WriteLine(@"*************************************************************");
            Console.ResetColor();
            Helper.msg("Press any key to run the simulation..", ConsoleColor.Yellow);
            Console.ReadKey();
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
                string generatedName = string.Empty;

                if (random.Next(0, 2) == 0)
                {
                    generatedName = GenerateName(i);
                    randomGenderForThisPerson = Person.SexType.Male;
                }
                else
                {
                    generatedName = GenerateName(i, false);
                    randomGenderForThisPerson = Person.SexType.Female;
                }

                Person eachPerson = new Person
                {
                    Id = i,
                    Name = generatedName,
                    Age = randomAgeForThisPerson,
                    Gender = randomGenderForThisPerson
                };

                this.Population.Add(eachPerson);
            }
        }

        /// <summary>
        /// Generate random names
        /// </summary>
        /// <param name="i">i is the id that attaches with the name</param>
        /// <param name="isMale">true for male name</param>
        /// <returns></returns>
        private string GenerateName(int i, bool isMale = true)
        {
            Random random = new();
            string generatedName;
            if (isMale)
            {
                int nameIndex = random.Next(0, commonMaleNames.Length);
                generatedName = $"{commonMaleNames[nameIndex]} {i}";
            }
            else {
                int nameIndex = random.Next(0, commonFemaleNames.Length);
                generatedName = $"{commonFemaleNames[nameIndex]} {i}";
            }
            return generatedName;
        }

        private bool IsPopulationWipedOut()
        {
            return !Population.Any(p => p.IsAlive);
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
                    if (person.Childrens.Count < Person._NO_OF_CHILDREN ||
                        spouse.Childrens.Count < Person._NO_OF_CHILDREN)
                    {
                        Random random = new Random();

                        // get the spouse

                        // get the next available id
                        int newId = getNextAvailableId();

                        // get random age for 10 to 25
                        int babyAge = 0;

                        // generate a random Gender for this person
                        Person.SexType randomGenderForThisPerson;
                        string generatedName = string.Empty;
                        if (random.Next(0, 2) == 0)
                        {
                            randomGenderForThisPerson = Person.SexType.Male;
                            generatedName = GenerateName(newId, isMale: true);
                        }
                        else
                        {
                            randomGenderForThisPerson = Person.SexType.Female;
                            generatedName = GenerateName(newId, isMale: false);
                        }

                        Person baby = new Person
                        {
                            Id = newId,
                            Name = generatedName,
                            Age = babyAge,
                            Gender = randomGenderForThisPerson
                        };

                        if (SHOW_INDIVISUAL_DETAIL)
                        {
                            Helper.msg($"{Person.PersonInfoString(person)} & {Person.PersonInfoString(spouse)} is having baby {Person.PersonInfoString(baby)}", ConsoleColor.Magenta);
                        }

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
        private bool DoesIdExist(int id)
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
            if (SHOW_INDIVISUAL_DETAIL)
            {
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
            }
            #endregion
        }

        /// <summary>
        /// Who ever is alive in the Population list will get 1 year incremented to their age.
        /// </summary>
        private void HappyBirthdayToAll()
        {
            for (int i = 0; i < Population.Count; i++)
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
        /// no gay marriages yet.
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
                    bool isMarriedNow = FindMateAndMarry(eachPerson);
                    if (!isMarriedNow && SHOW_INDIVISUAL_DETAIL)
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
                    if(SHOW_INDIVISUAL_DETAIL)
                        Helper.msg($"-> {Person.PersonInfoString(person)} got married to {Person.PersonInfoString(significantOther)})", ConsoleColor.Magenta);

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

                if(SHOW_INDIVISUAL_DETAIL)
                    Helper.msg($"{Person.PersonInfoString(person)} was married to {Person.PersonInfoString(significantOther!)}", ConsoleColor.Red);
                
                significantOther!.CurrentSpouseId = 0;
                person.CurrentSpouseId = 0;
            }
        }

        /// <summary>
        /// Gets the spouse Person instance
        /// </summary>
        /// <param name="person">Person instance you need to find the spouse on </param>
        /// <returns>returns the spouse Person object for the Person person object</returns>
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

        public void Dispose()
        {

            if (Population != null)
            {
                Population.Clear();
                Population = null;
            }
        }
    }
    /// <summary>
    /// Person class, this will hold information and functionality related to each single person
    /// </summary>
    public class Person
    {
        // person settings variables
        public static readonly int _DEATH_AGE               = 80;
        public static readonly int _FERTILITY_BEGIN_AGE     = 16;
        public static readonly int _MARRAGE_AGE             = 18;
        public static readonly int _FERTILITY_END_AGE       = 55;
        public static readonly int _NO_OF_CHILDREN          = 2;

        /// <summary>
        /// A list of Spouses, a person can only marry one person at a time but in the case of death of the spouse or a divorce etc.
        /// They can remarry. but all the spouses ex or current are stored in the list.
        /// The current active spouse can be found using Person.CurrentSpouseId
        /// </summary>
        public List<Person> Spouses { get; set; } = new List<Person>();

        /// <summary>
        /// These are all the Children belong to this person. biologically.
        /// even though a person is only allowed to have _NO_OF_CHILDREN many children, but in the case of the
        /// death of the spouse and remarrigae, They can have _NO_OF_CHILDREN more children.
        /// </summary>
        public List<Person> Childrens { get; set; } = new List<Person>();

        // properties
        public int Id { get; set; }

        private int _age;
        public int Age
        {
            get { return _age; }
            set
            {
                if (value == _age)
                    return;

                _age = value;
            }
        }

        public SexType Gender { get; set; }

        public string Name { get; set; }

        public int CurrentSpouseId { get; set; } = 0;

        public bool IsMarried
        {
            get
            {
                if (IsAlive && CurrentSpouseId > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // for simplicity the IsAlive is checked by looking at the age. this will be fixed in future versions.
        public bool IsAlive
        {
            get
            {
                if (Age >= _DEATH_AGE)
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
                if (Age >= _FERTILITY_BEGIN_AGE && Age <= _FERTILITY_END_AGE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // Methods
        public bool CanMarry()
        {
            if (Age >= _MARRAGE_AGE && IsAlive && CurrentSpouseId == 0)
                return true;
            else
                return false;
        }

        public bool SetSpouse(int spouseId)
        {
            CurrentSpouseId = spouseId;
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

    /// <summary>
    /// A helper to display messages on the screen, like messages with colors and error messages etc.
    /// </summary>
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


//// this region is only here for the functionality of moving the console application to the right for easier debugging experience
//#region MOVING THE CONSOLE TO LEFT
//const int SWP_NOSIZE = 0x0001;
//const int SWP_NOZORDER = 0x0004;
//const int SWP_SHOWWINDOW = 0x0040;

//[DllImport("kernel32.dll", SetLastError = true)]
//static extern IntPtr GetConsoleWindow();

//[DllImport("user32.dll", SetLastError = true)]
//static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

///// <summary>
///// this method is used to move the console application to x and y location. this functionality is not cross platform
///// and only specific to windows.
///// </summary>
///// <param name="x"></param>
///// <param name="y"></param>
//private static void MoveConsoleApplication(int x, int y)
//{
//    SetWindowPos(GetConsoleWindow(), IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
//}
//#endregion



//// move the application to the given coordinates on the screen for better debugging experience.
//Program.MoveConsoleApplication(1000, 0);
