using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using NsExcel = Microsoft.Office.Interop.Excel;

namespace WPFTask
{
    /// <summary>
    /// Логика взаимодействия для Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        private MainWindow _mainWindow;
        
        public Export()
        { 
            InitializeComponent();
        }
        
        public Export(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }
        /// <summary>
        ///selection of data for export
        /// </summary>
        
        public List<string[]> SelectData()    
        {
            string[] tempData = new string[6];
            tempData[0] = dateText.Text;
            tempData[1] = nameText.Text;
            tempData[2] = surnameText.Text;
            tempData[3] = lastNameText.Text;
            tempData[4] = cityText.Text;
            tempData[5] = countryText.Text;
            List<string[]> toExport = new List<string[]>();
            List<string[]> tempPeople = GetPeople();

            foreach (string[] data in tempPeople)
            {
                bool checkData = true;
                for (int i = 0; i < 6; i++)
                {
                    //check for equality the entered field and the table field
                    if (data[i + 1] != tempData[i])
                    {
                        if (tempData[i] == string.Empty)
                        {
                            continue;
                        }
                        else
                        {
                            checkData = false;
                            break;
                        }
                    }
                }
                if (checkData)
                {
                    toExport.Add(data);
                }
            }
            return toExport;
        }
        
        /// <summary>
        /// get list of people from _mainWindow listBox
        /// </summary>
        public List<string[]> GetPeople()
        {
            string[] tempArray = new string[7]; 
            List<string[]> personList = new List<string[]>();
            foreach (Person item in _mainWindow.peopleList.Items)
            {
                tempArray[0] = item.id.ToString();
                tempArray[1] = item.date.ToString();
                tempArray[2] = item.firstName;
                tempArray[3] = item.lastName;
                tempArray[4] = item.surname;
                tempArray[5] = item.city;
                tempArray[6] = item.country;
                personList.Add(tempArray);
            }
            return personList;
        }
        
        private void ExportToXML_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> toExport;
            toExport = SelectData();
            if (toExport != null) 
            { 
                ListToXML(toExport); 
            }
        }
        
        /// <summary>
        ///export to xml
        /// </summary>
        public void ListToXML(List<string[]> list)
        {
            progressBar.Visibility = Visibility;
            //set Minimum to 1 to represent the first file being copied
            progressBar.Minimum = 0;
            //set Maximum to the total number of files to copy
            progressBar.Maximum = list.Count;
            //set the initial value of the ProgressBar
            progressBar.Value = 0;

            var xEle = new XElement("People",
                from person in list
                select new XElement("Person",
                             new XAttribute("ID", person[0].ToString()),
                               new XElement("Date", person[1]),
                               new XElement("FirstName", person[2]),
                               new XElement("LastName", person[3]),
                               new XElement("Surname", person[4]),
                               new XElement("City", person[5]),
                               new XElement("Country", person[6])
                           ));
            progressBar.Value++;
            xEle.Save("People.xml");
        }
        
        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> toExport;
            toExport = SelectData();
            if (toExport != null)
            {
                ListToExcel(toExport);
            }
        }
        
        /// <summary>
        ///export to Excel
        /// </summary>
        public void ListToExcel(List<string[]> list)
        {
            //display the ProgressBar control
            progressBar.Visibility = Visibility;
            //set Minimum to 1 to represent the first file being copied
            progressBar.Minimum = 0;
            //set Maximum to the total number of files to copy
            progressBar.Maximum = list.Count;
            //set the initial value of the ProgressBar
            progressBar.Value = 0;

            NsExcel.Application ExcelApp = new NsExcel.Application();
            ExcelApp.Application.Workbooks.Add(Type.Missing);
            ExcelApp.Columns.ColumnWidth = 15;

            ExcelApp.Cells[1, 1] = "Date";
            ExcelApp.Cells[1, 2] = "First name";
            ExcelApp.Cells[1, 3] = "Last name";
            ExcelApp.Cells[1, 4] = "Surname";
            ExcelApp.Cells[1, 5] = "City";
            ExcelApp.Cells[1, 6] = "Country";

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Length - 1; j++)
                {
                    foreach (string[] strings in list)
                    {
                        ExcelApp.Cells[i + 2, j + 1] = strings[j + 1];
                    }
                }
                progressBar.Value++;
            }
            ExcelApp.Visible = true;
        }
    }
}
