﻿using Ed.it.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using System.Linq;

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

        cmd.CommandTimeout = 60;           // Time to wait for the execution' The default is 30 seconds

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
            string query = $@"INSERT INTO _User values('{user.Password}','{user.Name}','{user.Email}','{user.TeacherType}','{user.BDate}','{user.SchoolType}','{user.AboutMe}','{user.UrlPicture}','{user.Email.Split('@').First()}')";
            cmd = CreateCommand(query, con);
            numEffected += cmd.ExecuteNonQuery(); // execute the command

            for (int i = 0; i < user.TagsUser.Count; i++)
            {
                string query2 = $@"INSERT INTO _TagsUsedOn values({1},'{user.Email.Split('@').First()}', '{user.TagsUser[i]}')";
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
    internal int UploadContent(Content content)
    {
        try
        {
            con = Connect("DBConnectionString");
            int numEffected = 0;
            string query = $@"INSERT INTO _Content values('{content.ContentName}','{content.PathFile}','{content.ByUser}','{content.Description}','{content.UploadedDate}')";
            cmd = CreateCommand(query, con);
            numEffected += cmd.ExecuteNonQuery(); // execute the command
            //שליפת זהות התוכן שהועלה עכשיו
            query = $@"SELECT max(ContentID) FROM _Content";
            dt = new DataTable();
            da = new SqlDataAdapter(query, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            dt = ds.Tables[0];
            int ContentId =Convert.ToInt32(dt.Rows[0][0].ToString());
            //הכנסת תגים עבור התוכן
            for (int i = 0; i < content.TagsContent.Count; i++)
            {
                query = $@"INSERT INTO _ContentRelatedTo values('{content.TagsContent[i]}',{ContentId})";
                cmd = CreateCommand(query, con);
                numEffected += cmd.ExecuteNonQuery();
            }

            return ContentId;

           
        }
        catch (Exception ex)
        {
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

    /// <summary>
    /// אלגוריתם חכם-הצעת תכנים למשתמש
    /// </summary>
    internal List<Content> GetSuggestionsOfContents(string userName)
    {
        Content content = new Content();
        List<Content> SuggestionList = new List<Content>();//בניית רשימת התכנים המוצעים -מה שיוחזר בסוף
        DataTable ScoreTable = new DataTable();//טבלת עם התגים של היוזר לפי ניקוד
        DataTable ContentTagTable = new DataTable();//טבלה עבור כל התכנים של התגית המבוקשת
        try
        {
            //שלב 1- שליפת רשימת התגים שאוהב היוזר לפי ניקוד בסדר יורד
            con = Connect("DBConnectionString");
            string query = $@"SELECT *
                              FROM _TagsUsedOn
                              WHERE UserName='{userName}'
                              ORDER BY Score desc";
            da = new SqlDataAdapter(query, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ScoreTable = ds.Tables[0];

            //שלושה מחזורים באלגוריתם חכם
            string TagToSearch="";
            if (ScoreTable.Rows.Count == 0)
                return SuggestionList;
            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(ScoreTable.Rows[i]["TagName"].ToString()))
                    TagToSearch = ScoreTable.Rows[i]["TagName"].ToString();
                else
                    continue;

                //בניית השאילתה שמחזירה את כל התכנים של התגית המבוקשת בסדר יורד לפי כמות הלייקים
                //לא יוחזרו מצגות שהועלו על ידי המשתמש הנוכחי
                query = $@" SELECT *
                            FROM _Content C inner join _ContentRelatedTo R on C.ContentID=R.ContentID 
                            inner join( select count(*) as Likes,ContentID
			                            from _Liked
			                            group by ContentID) as L on C.ContentID=L.ContentID
                            WHERE TagName='{TagToSearch}' and C.ByUser<>'{userName}'
                            ORDER BY Likes desc";
                da = new SqlDataAdapter(query, con);
                ds = new DataSet();
                da.Fill(ds);
                ContentTagTable = ds.Tables[0];

                int HowManyContentsToAdd= ContentTagTable.Rows.Count;
                if (ContentTagTable.Rows.Count >= 3)//אם יותר משלוש יקח חמישים אחוז
                    HowManyContentsToAdd = (ContentTagTable.Rows.Count) / 2;//50%

                //הוספת התכנים לטבלת התכנים
                for (int j = 0; j < HowManyContentsToAdd; j++)
                {
                    //שליפת פרטים כללים על המצגת-רק להצגה 
                    if (!string.IsNullOrEmpty(ContentTagTable.Rows[j]["ContentID"].ToString()))
                    {
                        content.ContentID = Convert.ToInt32(ContentTagTable.Rows[j]["ContentID"]);
                        if (!SuggestionList.Exists(co => co.ContentID == content.ContentID))//רק אם לא מכיל כבר את התוכן
                        {
                            content.ContentName = ContentTagTable.Rows[j]["ContentName"].ToString();
                            content.Description = ContentTagTable.Rows[j]["Description"].ToString();
                            content.PathFile = ContentTagTable.Rows[j]["PathFile"].ToString();
                            content.PathFile = content.PathFile.Split('.').First()+"_1.jpg";//מציגים את התמונה הראשונה
                            SuggestionList.Add(content);//מוסיף לרשימת ההמלצות
                            content = new Content();
                        }
                    }                                                                
                }
            }
        }
        catch (Exception ex)
        {
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

        return SuggestionList;
    }

    /// <summary>
    /// הצעת תכנים לאורח-התכנים עם הכי הרבה לייקים ללא קשר לתגיות
    /// </summary>
    internal List<Content> GetSuggestionsOfContentsForGuest()
    {
        Content content = new Content();
        List<Content> SuggestionList = new List<Content>();//בניית רשימת התכנים המוצעים -מה שיוחזר בסוף
        DataTable ContentTagTable = new DataTable();
        try
        {
            con = Connect("DBConnectionString");
            //מביא את 10 התכנים עם הכי הרבה לייקים
            string query = $@"SELECT Top 10 C.ContentID,C.ContentName,C.PathFile,CAST(Description AS NVARCHAR(200)) as Description,L.Likes
                            FROM _Content C inner join _ContentRelatedTo R on C.ContentID=R.ContentID 
                            inner join( select count(*) as Likes,ContentID
			                            from _Liked
			                            group by ContentID) as L on C.ContentID=L.ContentID
                            Group by C.ContentID,C.ContentName,C.PathFile,L.Likes,CAST(Description AS NVARCHAR(200))
                            ORDER BY Likes desc";
            da = new SqlDataAdapter(query, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            ContentTagTable = ds.Tables[0];
           
            //הוספת התכנים לרשימה
            for (int j = 0; j < ContentTagTable.Rows.Count; j++)
            {
                //שליפת פרטים כללים על המצגת-רק להצגה 
                if (!string.IsNullOrEmpty(ContentTagTable.Rows[j]["ContentID"].ToString()))
                {
                    content.ContentID = Convert.ToInt32(ContentTagTable.Rows[j]["ContentID"]);
                    if (!SuggestionList.Exists(co => co.ContentID == content.ContentID))//רק אם לא מכיל כבר את התוכן
                    {
                        content.ContentName = ContentTagTable.Rows[j]["ContentName"].ToString();
                        content.Description = ContentTagTable.Rows[j]["Description"].ToString();
                        content.PathFile = ContentTagTable.Rows[j]["PathFile"].ToString();
                        content.PathFile = content.PathFile.Split('.').First() + "_1.jpg";//מציגים את התמונה הראשונה
                        SuggestionList.Add(content);//מוסיף לרשימת ההמלצות
                        content = new Content();
                    }
                }
            }
        }
        catch (Exception ex)
        {
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

        return SuggestionList;
    }

    /// <summary>
    /// חיפוש תכנים לפי תגית
    /// </summary>
    internal List<Content> Search(string TagToSearch)
    {
        Content content = new Content();
        List<Content> SuggestionList = new List<Content>();//בניית רשימת התכנים המוצעים -מה שיוחזר בסוף
        DataTable TagsRealetedTable = new DataTable();//טבלה עבור התגיות שמשתייכות לתגית המבוקשת
        DataTable ContentTagTable = new DataTable();//טבלה עבור כל התכנים של התגית המבוקשת

        try
        {
            //מציאת תגיות שהכי קשורות לתגית שרשמנו במנוע חיפוש
            //סופר מי 2 התווית שמתלווה הכי הרבה פעמים בתכנים בהם מופיעה גם התגית שאנחנו מחפשים
            //הראשונה כמובן תהיה התגית שאנחנו מחפשים
            string query = $@"SELECT top 3 COUNT(TagName) as TagsCount,TagName
                        FROM _ContentRelatedTo
                        WHERE ContentID in(
					                        select ContentID
					                        from _ContentRelatedTo
					                        where TagName='{TagToSearch}') 
                        GROUP by TagName
                        ORDER by TagsCount desc";
            da = new SqlDataAdapter(query, con);
            DataSet ds = new DataSet();
            da.Fill(ds);
            TagsRealetedTable = ds.Tables[0];

            for (int i = 0; i < TagsRealetedTable.Rows.Count; i++)
            {
                //בניית השאילתה שמחזירה את כל התכנים של התגית המבוקשת בסדר יורד לפי כמות הלייקים
                //לא יוחזרו מצגות שהועלו על ידי המשתמש הנוכחי
                query = $@" SELECT *
                            FROM _Content C inner join _ContentRelatedTo R on C.ContentID=R.ContentID 
                            inner join( select count(*) as Likes,ContentID
			                            from _Liked
			                            group by ContentID) as L on C.ContentID=L.ContentID
                            WHERE TagName='{TagsRealetedTable.Rows[i]["TagName"].ToString()}' 
                            ORDER BY Likes desc";
                da = new SqlDataAdapter(query, con);
                ds = new DataSet();
                da.Fill(ds);
                ContentTagTable = ds.Tables[0];

                //הוספת התכנים לטבלת התכנים
                for (int j = 0; j < ContentTagTable.Rows.Count; j++)
                {
                    //שליפת פרטים כללים על המצגת-רק להצגה 
                    if (!string.IsNullOrEmpty(ContentTagTable.Rows[j]["ContentID"].ToString()))
                    {
                        content.ContentID = Convert.ToInt32(ContentTagTable.Rows[j]["ContentID"]);
                        if (!SuggestionList.Exists(co => co.ContentID == content.ContentID))//רק אם לא מכיל כבר את התוכן
                        {
                            content.ContentName = ContentTagTable.Rows[j]["ContentName"].ToString();
                            content.Description = ContentTagTable.Rows[j]["Description"].ToString();
                            content.PathFile = ContentTagTable.Rows[j]["PathFile"].ToString();
                            content.PathFile = content.PathFile.Split('.').First() + "_1.jpg";//מציגים את התמונה הראשונה
                            SuggestionList.Add(content);//מוסיף לרשימת ההמלצות
                            content = new Content();
                        }
                    }
                }
       
            }
        }
        catch (Exception ex)
        {
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

        return SuggestionList;
    }


}