using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class CustomerRepository
    {
        public DataTable GetCustomers(string search = "", int status = -1)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Musteri_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200)
                        .Value = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();

                    cmd.Parameters.Add("@Durum", SqlDbType.Int)
                        .Value = status;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteriler getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteriler getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataRow GetCustomerById(int customerId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Musteri_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int)
                        .Value = customerId;

                    connection.Open();
                    adapter.Fill(table);
                }

                if (table.Rows.Count == 0)
                    return null;

                return table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri bilgisi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri bilgisi getirilirken hata oluştu: " + ex.Message);
            }
        }

        public int InsertCustomer(
            string fullName,
            string companyName,
            string authorizedPerson,
            string phone,
            string email,
            string taxNumber,
            string taxOffice,
            string address,
            string customerType,
            bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Musteri_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddCustomerParameters(
                        cmd,
                        fullName,
                        companyName,
                        authorizedPerson,
                        phone,
                        email,
                        taxNumber,
                        taxOffice,
                        address,
                        customerType,
                        isActive);

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri eklenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri eklenirken hata oluştu: " + ex.Message);
            }
        }

        public void UpdateCustomer(
            int customerId,
            string fullName,
            string companyName,
            string authorizedPerson,
            string phone,
            string email,
            string taxNumber,
            string taxOffice,
            string address,
            string customerType,
            bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Musteri_Guncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int)
                        .Value = customerId;

                    AddCustomerParameters(
                        cmd,
                        fullName,
                        companyName,
                        authorizedPerson,
                        phone,
                        email,
                        taxNumber,
                        taxOffice,
                        address,
                        customerType,
                        isActive);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public void SetCustomerActiveStatus(int customerId, bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Musteri_DurumGuncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int)
                        .Value = customerId;

                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri durumu değiştirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri durumu değiştirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataTable GetActiveCustomers()
        {
            return GetCustomers("", 1);
        }

        public DataTable GetCustomerStores(int customerId, int status = -1)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MusteriMagaza_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int)
                        .Value = customerId;

                    cmd.Parameters.Add("@Durum", SqlDbType.Int)
                        .Value = status;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri mağazaları getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri mağazaları getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataTable GetActiveCustomerStores(int customerId)
        {
            return GetCustomerStores(customerId, 1);
        }

        public DataRow GetCustomerStoreById(int customerStoreId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MusteriMagaza_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerStoreId", SqlDbType.Int)
                        .Value = customerStoreId;

                    connection.Open();
                    adapter.Fill(table);
                }

                if (table.Rows.Count == 0)
                    return null;

                return table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri mağazası getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri mağazası getirilirken hata oluştu: " + ex.Message);
            }
        }

        public int InsertCustomerStore(
            int customerId,
            string storeName,
            string city,
            string district,
            string address,
            string phone,
            string responsiblePerson,
            bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MusteriMagaza_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    AddCustomerStoreParameters(
                        cmd,
                        customerId,
                        storeName,
                        city,
                        district,
                        address,
                        phone,
                        responsiblePerson,
                        isActive);

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri mağazası eklenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri mağazası eklenirken hata oluştu: " + ex.Message);
            }
        }

        public void UpdateCustomerStore(
            int customerStoreId,
            string storeName,
            string city,
            string district,
            string address,
            string phone,
            string responsiblePerson,
            bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MusteriMagaza_Guncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerStoreId", SqlDbType.Int)
                        .Value = customerStoreId;

                    cmd.Parameters.Add("@StoreName", SqlDbType.NVarChar, 300)
                        .Value = storeName.Trim();

                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100)
                        .Value = GetNullableValue(city);

                    cmd.Parameters.Add("@District", SqlDbType.NVarChar, 100)
                        .Value = GetNullableValue(district);

                    cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500)
                        .Value = GetNullableValue(address);

                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 60)
                        .Value = GetNullableValue(phone);

                    cmd.Parameters.Add("@ResponsiblePerson", SqlDbType.NVarChar, 200)
                        .Value = GetNullableValue(responsiblePerson);

                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri mağazası güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri mağazası güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public void SetCustomerStoreActiveStatus(int customerStoreId, bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MusteriMagaza_DurumGuncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerStoreId", SqlDbType.Int)
                        .Value = customerStoreId;

                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Müşteri mağazası durumu değiştirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Müşteri mağazası durumu değiştirilirken hata oluştu: " + ex.Message);
            }
        }

        private void AddCustomerParameters(
            SqlCommand cmd,
            string fullName,
            string companyName,
            string authorizedPerson,
            string phone,
            string email,
            string taxNumber,
            string taxOffice,
            string address,
            string customerType,
            bool isActive)
        {
            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 300)
                .Value = fullName.Trim();

            cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 300)
                .Value = GetNullableValue(companyName);

            cmd.Parameters.Add("@AuthorizedPerson", SqlDbType.NVarChar, 200)
                .Value = GetNullableValue(authorizedPerson);

            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 60)
                .Value = GetNullableValue(phone);

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 400)
                .Value = GetNullableValue(email);

            cmd.Parameters.Add("@TaxNumber", SqlDbType.NVarChar, 50)
                .Value = GetNullableValue(taxNumber);

            cmd.Parameters.Add("@TaxOffice", SqlDbType.NVarChar, 100)
                .Value = GetNullableValue(taxOffice);

            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500)
                .Value = GetNullableValue(address);

            cmd.Parameters.Add("@CustomerType", SqlDbType.NVarChar, 50)
                .Value = string.IsNullOrWhiteSpace(customerType) ? "Toptan" : customerType.Trim();

            cmd.Parameters.Add("@IsActive", SqlDbType.Bit)
                .Value = isActive;
        }

        private void AddCustomerStoreParameters(
            SqlCommand cmd,
            int customerId,
            string storeName,
            string city,
            string district,
            string address,
            string phone,
            string responsiblePerson,
            bool isActive)
        {
            cmd.Parameters.Add("@CustomerId", SqlDbType.Int)
                .Value = customerId;

            cmd.Parameters.Add("@StoreName", SqlDbType.NVarChar, 300)
                .Value = storeName.Trim();

            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100)
                .Value = GetNullableValue(city);

            cmd.Parameters.Add("@District", SqlDbType.NVarChar, 100)
                .Value = GetNullableValue(district);

            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500)
                .Value = GetNullableValue(address);

            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 60)
                .Value = GetNullableValue(phone);

            cmd.Parameters.Add("@ResponsiblePerson", SqlDbType.NVarChar, 200)
                .Value = GetNullableValue(responsiblePerson);

            cmd.Parameters.Add("@IsActive", SqlDbType.Bit)
                .Value = isActive;
        }

        private object GetNullableValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            return value.Trim();
        }
    }
}