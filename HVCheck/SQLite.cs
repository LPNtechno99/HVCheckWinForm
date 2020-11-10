using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;
using HVCheck.Models;

namespace HVCheck
{
    public class SQLite
    {
        public static string _DBFilePath = Application.StartupPath + "\\DBHVCheck.db";
        public bool _SqliteConnected = false;
        public bool _checkconnect = false;
        SQLiteConnection con = new SQLiteConnection("Datasource=" + _DBFilePath + ";Version=3;Compress=True;");

        private static SQLite _instance;
        /// <summary>
        /// return a instance of class SQLite
        /// </summary>
        /// <returns></returns>
        public static SQLite Instance()
        {
            if (_instance == null)
            {
                _instance = new SQLite();
            }
            return _instance;
        }
        /// <summary>
        /// Do check have see connection?
        /// </summary>
        public void CheckConnect()
        {
            try
            {
                con.Open();
                if (con.State == ConnectionState.Open)
                {
                    _checkconnect = true;
                    MessageBox.Show("OK");
                    con.Close();
                }
                else
                {
                    _checkconnect = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _checkconnect = false;
            }
        }
        /// <summary>
        /// Function Open Connect to Database sqlite
        /// </summary>
        public void OpenConnection()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                    _SqliteConnected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Function Close Connect to Database sqlite
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    _SqliteConnected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Lấy dữ liệu
        public List<string> LayDanhSachDaiLy()
        {
            List<string> _lst = new List<string>();
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "SELECT TenDL FROM DanhSachDaiLy";
                using (SQLiteCommand command = new SQLiteCommand(query, con))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                _lst.Add(reader.GetValue(0).ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                CloseConnection();
                return _lst;
            }
            else return null;
        }
        public List<List<string>> LayToanBoBangDanhSachDaiLy()
        {
            List<string> _lst1 = new List<string>();
            List<string> _lst2 = new List<string>();
            List<List<string>> _lst = new List<List<string>>();
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "SELECT *FROM DanhSachDaiLy";
                using (SQLiteCommand command = new SQLiteCommand(query, con))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                _lst1.Add(reader.GetValue(1).ToString());
                                _lst2.Add(reader.GetValue(2).ToString());
                            }
                            _lst.Add(_lst1);
                            _lst.Add(_lst2);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                CloseConnection();
                return _lst;
            }
            else return null;
        }
        /// <summary>
        /// Tạo bảng cho form
        /// </summary>
        /// <param name="strquery"></param>
        /// <returns></returns>
        public DataTable TaoBang(string strquery)
        {
            try
            {
                OpenConnection();
                DataTable dt = new DataTable();
                using (SQLiteCommand command = new SQLiteCommand(strquery, con))
                {
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(command))
                    {
                        da.Fill(dt);
                    }
                }
                CloseConnection();
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public DuLieuSanPham LayDuLieuSanPham(string maSP)
        {
            DuLieuSanPham duLieuSanPham = new DuLieuSanPham();
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "SELECT *FROM DuLieuSanPham WHERE MaSP=@MaSP";
                using (SQLiteCommand command = new SQLiteCommand(query, con))
                {
                    command.Parameters.AddWithValue("MaSP", maSP);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                duLieuSanPham.ID = reader.GetInt32(0);
                                duLieuSanPham.TimeStamp = reader.GetString(1);
                                duLieuSanPham.Time = reader.GetString(2);
                                duLieuSanPham.MaDL = reader.GetString(3);
                                duLieuSanPham.TenDL = reader.GetString(4);
                                duLieuSanPham.MaSP = reader.GetString(5);
                            }
                            CloseConnection();
                        }
                        catch (Exception ex)
                        {
                            CloseConnection();
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                return duLieuSanPham;
            }
            else return null;
        }

        public DanhSachDaiLy LayDaiLy(string tenDL)
        {
            DanhSachDaiLy dsDaiLy = new DanhSachDaiLy();
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "SELECT *FROM DanhSachDaiLy WHERE TenDL=@TenDL";
                using (SQLiteCommand command = new SQLiteCommand(query, con))
                {
                    command.Parameters.AddWithValue("TenDL", tenDL);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                dsDaiLy.ID = reader.GetInt32(0);
                                dsDaiLy.MaDL = reader.GetString(1);
                                dsDaiLy.TenDL = reader.GetString(2);
                            }
                            CloseConnection();
                        }
                        catch (Exception ex)
                        {
                            CloseConnection();
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                return dsDaiLy;
            }
            else return null;
        }
        #endregion

        #region Thêm mới dữ liệu
        public void ThemDaiLy(string maDL, string tenDL)
        {
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "INSERT INTO DanhSachDaiLy (MaDL,TenDL) VALUES (@MaDL,@TenDL)";
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(query, con))
                    {
                        command.Parameters.AddWithValue("MaDL", maDL);
                        command.Parameters.AddWithValue("TenDL", tenDL);
                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                            MessageBox.Show("Thêm thành công");
                    }
                    CloseConnection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    CloseConnection();
                }
            }
        }
        public void ThemDuLieuSoLuongDat(string timestamp, string time, string maDL, string tenDL, int soluongdat)
        {
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "INSERT INTO DuLieuSoLuongDat (TimeStamp,Time,MaDL,TenDL,SoLuongDat) VALUES "
                    + "(@TimeStamp,@Time,@MaDL,@TenDL,@SoLuongDat)";
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(query, con))
                    {
                        command.Parameters.AddWithValue("TimeStamp", timestamp);
                        command.Parameters.AddWithValue("Time", time);
                        command.Parameters.AddWithValue("MaDL", maDL);
                        command.Parameters.AddWithValue("TenDL", tenDL);
                        command.Parameters.AddWithValue("SoLuongDat", soluongdat);
                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                            MessageBox.Show("OK");
                    }
                    CloseConnection();
                }
                catch (Exception ex)
                {
                    CloseConnection();
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void ThemDuLieuSanPham(string timestamp, string time, string maDL, string tenDL, string maSP)
        {
            OpenConnection();
            if (_SqliteConnected)
            {
                string query = "INSERT INTO DuLieuSanPham (TimeStamp,Time,MaDL,TenDL,MaSP) VALUES (@TimeStamp,@Time,@MaDL,@TenDL,@MaSP)";
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(query, con))
                    {
                        command.Parameters.AddWithValue("TimeStamp", timestamp);
                        command.Parameters.AddWithValue("Time", time);
                        command.Parameters.AddWithValue("MaDL", maDL);
                        command.Parameters.AddWithValue("TenDL", tenDL);
                        command.Parameters.AddWithValue("MaSP", maSP);
                        command.ExecuteNonQuery();
                    }
                    CloseConnection();
                }
                catch (Exception ex)
                {
                    CloseConnection();
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion

        #region Cập nhật dữ liệu
        #endregion

        #region Xóa Dữ Liệu
        public void XoaTheoID(string query)
        {
            OpenConnection();
            if (_SqliteConnected)
            {
                using (SQLiteCommand command = new SQLiteCommand(query, con))
                {
                    int res = command.ExecuteNonQuery();
                    if (res > 0)
                        MessageBox.Show("Xóa thành công");
                }
            }
        }
        #endregion
    }
}
