using Akka.Pattern;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/* Title: ThinkLP 
 * This class is to add, remove employees data and return a list of active or terminated employees
 * Author: Yumi Lee
 * Create date: March 21, 2020
 */

namespace YumiLee
{
    public class Employee
    {
        private long empId;
        private string firstName;
        private string lastName;
        private DateTime dateTerminate;

        static List<Employee> employeeList;

        public Employee()
        {
        }

        public Employee (long empId, string firstName, string lastName, DateTime dateTerminate)
        {
            this.empId = empId;
            this.firstName = firstName;
            this.lastName = lastName;
            this.dateTerminate = dateTerminate;      
        }

        public long getEmpId()
        {
            return empId;
        }

        public string getFirstName()
        {
            return firstName;
        }

        public string getLastName()
        {
            return lastName;
        }

        public DateTime getDateTerminate()
        {
            return dateTerminate;
        }
        

        /// <summary>
        /// This method validates user inputs and set default terminate date as 31/12/9999 for 
        /// permanent employees
        /// , and inserts new employee data to the database
        /// </summary>
        /// <returns></returns>
        public string AddEmployee()
        {
            StringBuilder messageToUI = new StringBuilder();

            if (this.firstName == null || this.firstName =="")
            {
                messageToUI.AppendLine("First Name is required");
            }
            if (this.lastName == null || this.lastName == "")
            {
                messageToUI.AppendLine("Last Name is required");
            }
            if(this.dateTerminate == null)
            {
                this.dateTerminate = DateTime.MaxValue; //To set as a permanent employee
            }

            string query = "INSERT INTO [Employee](empId, firstName,lastName, dateTerminate) " +
                "VALUES ('" + @empId + "','" + @firstName + "','" + @lastName + "','" + @dateTerminate + "')";

            SqlConnection sqlConnection = null;

            try
            {
                sqlConnection = new SqlConnection(GetDBConnectionString());
                sqlConnection.Open();
                SqlCommand cmd = new SqlCommand(query, sqlConnection);

                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@empId", Value = empId });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@firstName", Value = firstName });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@lastName", Value = lastName });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@dateTerminate", Value = dateTerminate });

                SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                sqlAdapter.InsertCommand = new SqlCommand(query, sqlConnection);
                sqlAdapter.InsertCommand.ExecuteNonQuery();

                cmd.Dispose();
                sqlConnection.Close();

                messageToUI.AppendLine("Successfully added");
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            return messageToUI.ToString();

        }

        /// <summary>
        /// This method for deleting user data by searching employee id. 
        /// </summary>
        /// <param name="userInputId"></param>
        public string RemoveEmployee(long userInputId)
        {
            this.empId = userInputId;

            string query = "DELETE FROM [Employee] WHERE empId = ('" + @empId + "')";

            SqlConnection sqlConnection = null;
            SqlCommand cmd = null;
            int deletedCount;

            try
            {
                sqlConnection = new SqlConnection(GetDBConnectionString());
                sqlConnection.Open();
                cmd = new SqlCommand(query, sqlConnection);

                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@empId", Value = empId });

                SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                sqlAdapter.DeleteCommand = new SqlCommand(query, sqlConnection);
                deletedCount = sqlAdapter.DeleteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                cmd.Dispose();
                sqlConnection.Close();
            }
            
            if (deletedCount > 0)
            {
                return "The record is deleted";
            }
            else
            {
                return "There is no matching employee id";
            }
            
        }

        /// <summary>
        /// This method is for returning a list of employee who are active.
        /// </summary>
        /// <returns>List<Employee></returns>
        public static List<Employee> ReturnActiveEmployees()
        {

            DateTime currentDate = DateTime.Now;
            string query = "SELECT * FROM [Employee] WHERE dateTerminate >= @currentDate";

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataReader read = null;

            try
            {
                sqlConnection = new SqlConnection(GetDBConnectionString());
                sqlConnection.Open();
                sqlCommand = new SqlCommand(query, sqlConnection);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@currentDate";
                param.Value = currentDate;
                sqlCommand.Parameters.Add(param);

                
                read = sqlCommand.ExecuteReader();

                if (employeeList != null)
                {
                    employeeList = null;
                }

                employeeList = new List<Employee>();

                while (read.Read())
                {
                    long empId = Convert.ToInt64(read.GetValue(0));
                    string firstName = read.GetValue(1).ToString();
                    string lastName = read.GetValue(2).ToString();
                    DateTime date = DateTime.Parse(read.GetValue(3).ToString());
                    employeeList.Add(new Employee(empId, firstName, lastName, date));

                }
            }
            catch (Exception)
            {
               throw new IllegalStateException("Error from sql command");
            }
            finally
            {
                read.Close();
                sqlCommand.Dispose();
                sqlConnection.Close();
            }

            return employeeList;

        }

        /// <summary>
        /// This method is for returning a list of employee who are terminated last month.
        /// </summary>
        /// <returns>List<Employee></returns>
        public static List<Employee> ReturnTerminatedEmployeesLastMonth()
        {

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var dateLastMonthStart = month.AddMonths(-1);
            var dateLastMonthEnd = month.AddDays(-1);
            var lastPlusOneday = dateLastMonthEnd.AddDays(1);

            string query = "SELECT * FROM [Employee] WHERE dateTerminate >= @dateLastMonthStart " +
                "AND dateTerminate < @lastPlusOneday";

            SqlConnection sqlConnection = null;
            SqlDataReader read = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(GetDBConnectionString());
                sqlConnection.Open();

                sqlCommand = new SqlCommand(query, sqlConnection);

                SqlParameter param = new SqlParameter();
                sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@dateLastMonthStart", 
                    Value = dateLastMonthStart });
                sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@lastPlusOneday", 
                    Value = lastPlusOneday });

                
                read = sqlCommand.ExecuteReader();


                if (employeeList != null)
                {
                    employeeList = null;
                }

                employeeList = new List<Employee>();

                while (read.Read())
                {
                    long empId = Convert.ToInt64(read.GetValue(0));
                    string firstName = read.GetValue(1).ToString();
                    string lastName = read.GetValue(2).ToString();
                    DateTime date = DateTime.Parse(read.GetValue(3).ToString());
                    employeeList.Add(new Employee(empId, firstName, lastName, date));

                }
            }
            catch (Exception)
            {
                throw new IllegalStateException("Error from sql command");
            }
            finally
            {
                read.Close();
                sqlCommand.Dispose();
                sqlConnection.Close();
            }

            return employeeList;

        }

        /// <summary>
        /// This method is called when Database connection string is needed.
        /// </summary>
        /// <returns>string connection string</returns>
        public static string GetDBConnectionString()
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" +
                "C:\\ThinkLP\\YumiLee\\YumiLee\\Database.mdf;Integrated Security=True";

            return connectionString;
        }

 }
}
