using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WPFTask
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        readonly IFileService fileService;
        readonly IDialogService dialogService;
        readonly MyDbContext db;

        StringBuilder errorMessages = new StringBuilder();

        public ObservableCollection<Person> Persons { get; set; }

        IEnumerable<Person> people;
        public List<string[]> data = new List<string[]>();

        RelayCommand addCommand;
        RelayCommand editCommand;
        RelayCommand deleteCommand;

        private MainWindow _mainWin { get; set; }

        public IEnumerable<Person> People
        {
            get
            {
                return people;
            }
            set
            {
                people = value;
                OnPropertyChanged("People");
            }
        }

        public ApplicationViewModel(MainWindow mainWindow, IDialogService dialogService, IFileService fileService)
        {
            this.dialogService = dialogService;
            this.fileService = fileService;
            _mainWin = mainWindow;
            try
            {
                db = new MyDbContext();
                db.Persons.Load();
                People = db.Persons.Local.ToBindingList();

                foreach (Person person in People)
                {
                    _mainWin.peopleList.Items.Add(person);
                }
            }
            catch(SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n");
                }
                MessageBox.Show(errorMessages.ToString());
            }
        }

        /// <summary>
        /// read data from .csv
        /// </summary>
        public async Task ReadCSVAsync()
        {
            string filePath = null;
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                }
                using (TextFieldParser textFieldParser = new TextFieldParser(filePath))
                {

                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.SetDelimiters(";");
                    while (!textFieldParser.EndOfData)
                    {
                        Person person = new Person();
                        try
                        {
                            string[] dataAboutPeople = textFieldParser.ReadFields();
                            person.date = Convert.ToDateTime(dataAboutPeople[0]);
                            person.firstName = dataAboutPeople[1].ToString();
                            person.lastName = dataAboutPeople[2].ToString();
                            person.surname = dataAboutPeople[3].ToString();
                            person.city = dataAboutPeople[4].ToString();
                            person.country = dataAboutPeople[5].ToString();
                            await WriteToSqlAsync(person);
                            _mainWin.peopleList.Items.Add(person);
                        }
                        catch
                        {
                            MessageBox.Show("File content error!");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("You didn't selest a file!");
            }
        }

        /// <summary>
        /// async writing method 
        /// </summary>
        public async Task WriteToSqlAsync(Person person)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.Persons.Add(person);
                db.SaveChanges();
            }
            
        }

        /// <summary>
        /// processing add command
        /// </summary>
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      NewPersonWindow newPersonWindow = new NewPersonWindow(new Person());
                      if (newPersonWindow.ShowDialog() == true)
                      {
                          Person person = newPersonWindow.Person;
                          bool check = CheckData(newPersonWindow.Person);
                          if (check)
                          {
                              db.Persons.Add(person);
                              _mainWin.peopleList.Items.Add(person);
                              db.SaveChanges();
                          }
                          else
                          {
                              MessageBox.Show("You enter incorrect data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                          }
                      }
                  }));
            }
        }

        /// <summary>
        /// processing edit command
        /// </summary>
        public RelayCommand EditCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((selectedItem) =>
                  {
                      if (selectedItem == null) return;
                      // get the selected object
                      Person person = selectedItem as Person;

                      NewPersonWindow newPersonWindow = new NewPersonWindow(new Person
                      {
                          Id = person.Id,
                          Date = person.Date,
                          FirstName = person.FirstName,
                          LastName = person.LastName,
                          Surname = person.Surname,
                          City = person.City,
                          Country = person.Country
                      });

                      if (newPersonWindow.ShowDialog() == true)
                      {
                          // get the changed object
                          person = db.Persons.Find(newPersonWindow.Person.Id);
                          if (person != null)
                          {
                              bool check = CheckData(newPersonWindow.Person);
                              if (check)
                              {
                                  person.Date = newPersonWindow.Person.Date;
                                  person.FirstName = newPersonWindow.Person.FirstName;
                                  person.LastName = newPersonWindow.Person.LastName;
                                  person.Surname = newPersonWindow.Person.Surname;
                                  person.City = newPersonWindow.Person.City;
                                  person.Country = newPersonWindow.Person.Country;
                                  db.Entry(person).State = EntityState.Modified;
                                  db.SaveChanges();
                              }
                              else
                              {
                                  MessageBox.Show("You enter incorrect data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                              }
                          }
                      }
                  }));
            }
        }

        /// <summary>
        /// processing delete command
        /// </summary>
        public RelayCommand DeleteCommand
        {
            get
            {
                return deleteCommand ??
                  (deleteCommand = new RelayCommand((selectedItem) =>
                  {
                      if (selectedItem == null) return;
                      // get the selected object
                      Person person = selectedItem as Person;
                      db.Persons.Remove(person);
                      _mainWin.peopleList.Items.Remove(person);
                      db.SaveChanges();
                  }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        bool CheckData(Person person)
        {
            string firstNamePattern = @"^[A-Z][a-z]{4-8}";
            string lastNamePattern = @"^[A-Z][a-z]{4-10}";
            string surnamePattern = @"^[A-Z][a-z]{4-12}";
            string cityPattern = @"^[A-Z][a-z]{5-9}";
            string countryPattern = @"^[A-Z][a-z]{5-12}";
            if (person.Date == null || string.IsNullOrEmpty(person.FirstName) ||
                string.IsNullOrEmpty(person.LastName) || string.IsNullOrEmpty(person.Surname) ||
                string.IsNullOrEmpty(person.City) || string.IsNullOrEmpty(person.Country))
            {
                return false;
            }
            if (Regex.IsMatch(person.FirstName, firstNamePattern, RegexOptions.IgnoreCase) ||
            Regex.IsMatch(person.LastName, lastNamePattern, RegexOptions.IgnoreCase) ||
            Regex.IsMatch(person.Surname, surnamePattern, RegexOptions.IgnoreCase) ||
            Regex.IsMatch(person.City, cityPattern, RegexOptions.IgnoreCase) ||
            Regex.IsMatch(person.Country, countryPattern, RegexOptions.IgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
