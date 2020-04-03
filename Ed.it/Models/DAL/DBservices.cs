﻿using Ed.it.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

/// <summary>
/// DBServices is a class created by me to provides some DataBase Services
/// </summary>
public class DBservices
{
    public SqlDataAdapter da;
    public DataTable dt;
    SqlConnection con;
    SqlCommand cmd;
    int counter = 0;

    public DBservices()
    {

    }



    //--------------------------------------------------------------------------------------------------
    // This method creates a connection to the database according to the connectionString name in the web.config 
    //--------------------------------------------------------------------------------------------------
    public SqlConnection Connect(String conString)
    {
        try
        {
            // read the connection string from the configuration file
            string cStr = WebConfigurationManager.ConnectionStrings[conString].ConnectionString;
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }
        catch(Exception ex)
        {
            throw (ex);
        }
  
    }


    //---------------------------------------------------------------------------------
    // Create the SqlCommand
    //---------------------------------------------------------------------------------
    private SqlCommand CreateCommand(String CommandSTR, SqlConnection con)
    {

        SqlCommand cmd = new SqlCommand(); // create the command object

        cmd.Connection = con;              // assign the connection to the command object

        cmd.CommandText = CommandSTR;      // can be Select, Insert, Update, Delete 

        cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

        cmd.CommandType = System.Data.CommandType.Text; // the type of the command, can also be stored procedure

        return cmd;
    }

    /// <summary>
    /// יצירת משתמש חדש במערכת
    /// </summary>
    internal int CreateUser(User user)
    {
        try
        {
            con = Connect("DBConnectionString");
            int numEffected = 0;
            string query = $@"INSERT INTO _User values('{user.Password}','{user.Name}','{user.Email}','{user.TeacherType}','{user.BDate}','{user.SchoolType}','{user.AboutMe}','{user.UrlPicture}')";
            cmd = CreateCommand(query, con);
            numEffected += cmd.ExecuteNonQuery(); // execute the command

            for (int i = 0; i < user.TagsUser.Count; i++)
            {
                string query2 = $@"INSERT INTO _UseOn values({1},'{user.Email}', '{user.TagsUser[i]}')";
                cmd= CreateCommand(query2, con);
                numEffected += cmd.ExecuteNonQuery();
            }
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();

            }
        }
    }


    /// <summary>
    /// בודק אם משתמש קיים
    /// </summary>
    internal User GetUserDetails(string Email, string Password)
    {
        User user = new User();
        con = Connect("DBConnectionString");                                             //שליפת זהות התוכן שהועלה עכשיו
        string query = $@"SELECT * 
                          FROM _User
                          WHERE Email='{Email}' and Password='{Password}'";
        DataTable dataTable = new DataTable();
        da = new SqlDataAdapter(query, con);
        SqlCommandBuilder builder = new SqlCommandBuilder(da);
        DataSet ds = new DataSet();
        da.Fill(ds);
        dataTable = ds.Tables[0];
        cmd = CreateCommand(query, con);
        if (dataTable.Rows.Count != 0)//אם משתמש קיים מבצע השמה לכל השדות
        {
            //user.Name = dataTable.Rows[0]["Name"].ToString();
            //user.UserName = dataTable.Rows[0]["UserName"].ToString();
            user.Name= dataTable.Rows[0]["Name"].ToString();
            user.Password = dataTable.Rows[0]["Password"].ToString();
            user.UrlPicture= dataTable.Rows[0]["UrlPicture"].ToString();
            user.SchoolType= dataTable.Rows[0]["SchoolType"].ToString();
            user.TeacherType = dataTable.Rows[0]["TeacherType"].ToString();
            user.Email = dataTable.Rows[0]["Email"].ToString();
            user.BDate=  dataTable.Rows[0]["Bdate"].ToString();
            user.AboutMe= dataTable.Rows[0]["AboutMe"].ToString();
            
            return user;
        }
        else
            return null;

    }

    /// <summary>
    /// העלאת תוכן ע"י משתמש
    /// </summary>
    internal int UploadContent(string userId, Content content)
    {
        try
        {
            con = Connect("DBConnectionString");
            int numEffected = 0;
            string query = $@"INSERT INTO _Teacher values('{content.ContentName}','{content.PathFile}','{content.Description}','{content.UploadedDate}')";
            cmd = CreateCommand(query, con);
            numEffected += cmd.ExecuteNonQuery(); // execute the command
            //שליפת זהות התוכן שהועלה עכשיו
            query = $@"SELECT max(ContentID) FROM _Content";
            dt = new DataTable();
            DataSet ds = new DataSet();
            da.Fill(ds);
            dt = ds.Tables[0];
            int ContentId =Convert.ToInt32(dt.Rows[0][0].ToString());
            //הכנסת תגים עבור התוכן
            for (int i = 0; i < content.TagsContent.Count; i++)
            {
                query = $@"INSERT INTO _RelatedTo values('{content.TagsContent[i]}',{ContentId})";
                numEffected += cmd.ExecuteNonQuery();
            }

            return numEffected;

           
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();

            }
        }
    }

    public List<string> GetTags()
    {
        List<string> Tags = new List<string>();
        SqlConnection con = null;
        string query = "";
        try
        {
            con = Connect("DBConnectionString");
            query = "select * from _TagsTest";
            cmd = CreateCommand(query, con);
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    string tag;
                    tag = (string)dr["TagName"];
                    Tags.Add(tag);
                }
            }

        }

        catch (Exception ex)
        {
            // write errors to log file
            // try to handle the error
            throw ex;
        }

        finally
        {
            if (con != null)
            {
                con.Close();
            }
        }
        return Tags;// מחזיר אובייקט מסוג DBServices
    }

    public int UpdateDetails(User NewUser)
    {
        try
        {
            con = Connect("DBConnectionString");
            int numEffected = 0;
            string query = $@"UPDATE  _User
                            SET Password='{NewUser.Password}', Name='{NewUser.Name}', TeacherType='{NewUser.TeacherType}', BDate='{NewUser.BDate}', SchoolType='{NewUser.SchoolType}', AboutMe='{NewUser.AboutMe}'
                            WHERE Email='{NewUser.Email}'";
            cmd = CreateCommand(query, con);
            numEffected =cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();

            }
        }
    }
}