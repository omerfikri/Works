﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Data.OleDb;
using System.Xml;

namespace odev
{
    public partial class admin : Form
    {
        int id;
        SqlConnection baglan = new SqlConnection("Data Source=DESKTOP-ORFTL34;Initial Catalog=Sqlyazilimyapimi;Integrated Security=True");
       
        public admin()
        {
            InitializeComponent();
            goster_urun();
            goster_para();
        }
        private void goster_urun()
        {
            lV_urun_onay.Items.Clear();
            baglan.Open();
            SqlCommand komut = new SqlCommand();
            komut.CommandText = "Select * From Onaylama";
            komut.Connection = baglan;

            SqlDataAdapter adap = new SqlDataAdapter(komut);
            DataTable tablo = new DataTable();

            adap.Fill(tablo);

            for (int i = 0; i < tablo.Rows.Count; i++)
            {
                lV_urun_onay.Items.Add(tablo.Rows[i]["id"].ToString());
                lV_urun_onay.Items[i].SubItems.Add(tablo.Rows[i]["kid"].ToString());
                lV_urun_onay.Items[i].SubItems.Add(tablo.Rows[i]["KAd"].ToString()+" "+ tablo.Rows[i]["KSoyad"].ToString());
                lV_urun_onay.Items[i].SubItems.Add(tablo.Rows[i]["Adi"].ToString());
                lV_urun_onay.Items[i].SubItems.Add(tablo.Rows[i]["Fiyat"].ToString());
                lV_urun_onay.Items[i].SubItems.Add(tablo.Rows[i]["UMiktar"].ToString());
            }
            baglan.Close();
        }

        private void goster_para()
        {
            lV_para_onay.Items.Clear();
            baglan.Open();
            SqlCommand komut = new SqlCommand();
            komut.CommandText = "Select * From OnayPara";
            komut.Connection = baglan;

            SqlDataAdapter adap = new SqlDataAdapter(komut);
            DataTable tablo = new DataTable();

            adap.Fill(tablo);

            for (int i = 0; i < tablo.Rows.Count; i++)
            {

                lV_para_onay.Items.Add(tablo.Rows[i]["id"].ToString());
                lV_para_onay.Items[i].SubItems.Add(tablo.Rows[i]["KAd"].ToString());
                lV_para_onay.Items[i].SubItems.Add(tablo.Rows[i]["ParaBirim"].ToString());
                lV_para_onay.Items[i].SubItems.Add(tablo.Rows[i]["KPara"].ToString());
            }
            baglan.Close();
        }
      
        private void listView1_DoubleClick_1(object sender, EventArgs e)
        {
            string ad = lV_urun_onay.SelectedItems[0].SubItems[3].Text;
            decimal para = Convert.ToDecimal(lV_urun_onay.SelectedItems[0].SubItems[4].Text);
            int stok = Convert.ToInt32(lV_urun_onay.SelectedItems[0].SubItems[5].Text);
            
            id = int.Parse(lV_urun_onay.SelectedItems[0].SubItems[0].Text);
            int saticiid = int.Parse(lV_urun_onay.SelectedItems[0].SubItems[1].Text); 

            DialogResult dialogResult = MessageBox.Show("Kabul Ediyor Musunuz?", "Onay", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                SqlCommand com = new SqlCommand("exec adminOnayUrun '" + id + "'", baglan);
                baglan.Open();
                com.ExecuteNonQuery();
                baglan.Close();
                goster_urun();

                talep(ad,para,stok,saticiid);
           
            }
            else if (dialogResult == DialogResult.No)
            {
                SqlCommand cmd = new SqlCommand("exec urunOnaylamamak '"+id+"'", baglan);
                baglan.Open();
                cmd.ExecuteNonQuery();
                baglan.Close();
                goster_urun();
            }
            
        }
        private void talep(string ad, decimal para, int stok, int saticiid)
        { 
            SqlCommand komut = new SqlCommand();
            SqlDataReader rd;
            baglan.Open();
            komut.Connection = baglan;
            komut.CommandText = "Select * From Talep Where UrunAd='" + ad + "' AND Fiyat>='" + para + "' AND Miktar<='" + stok + "'";
            rd = komut.ExecuteReader();

            if (rd.Read())
            {

                if (Convert.ToBoolean(rd["kontrol"]) == false)
                {
                    int miktar = Convert.ToInt32(rd["Miktar"]);
                    decimal fiyat = Convert.ToDecimal(rd["Fiyat"]);
                    int userid = Convert.ToInt32(rd["UseriD"]);
                    int talepid = Convert.ToInt32(rd["TalepiD"]);

                    SqlCommand com1 = new SqlCommand("exec Talepler '" + miktar + "','" + saticiid + "','" + userid + "','" + fiyat + "','" + talepid + "'", baglan);
                    com1.ExecuteNonQuery();
                }
            }
            baglan.Close();
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
           
                string birim;
                decimal deger;

                id = int.Parse(lV_para_onay.SelectedItems[0].SubItems[0].Text);
                DialogResult dialogResult = MessageBox.Show("Kabul Ediyor Musunuz?", "Onay", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {

                    birim = (lV_para_onay.SelectedItems[0].SubItems[2].Text.ToString());

                    deger=Para_birim(birim);
                                 
                    SqlCommand com = new SqlCommand("exec adminOnayPara '" + id +"','" + deger +  "'", baglan);
                    baglan.Open();
                    com.ExecuteNonQuery();
                    baglan.Close();
                    goster_para();

                }
                else if (dialogResult == DialogResult.No)
                {
                    SqlCommand cmd = new SqlCommand("exec paraOnaylamamak '" + id + "'", baglan);
                    baglan.Open();
                    cmd.ExecuteNonQuery();
                    baglan.Close();
                    goster_para();
                }
        }
        private decimal Para_birim(string birim)
        {
            decimal dolar;
            decimal euro;
            decimal sterlin;
            decimal deger;

            XmlDocument xmlVerisi = new XmlDocument();
            xmlVerisi.Load("http://www.tcmb.gov.tr/kurlar/today.xml");
            
            dolar = Convert.ToDecimal(xmlVerisi.SelectSingleNode(string.Format("Tarih_Date/Currency[@Kod='{0}']/ForexSelling", "USD")).InnerText.Replace('.', ','));
            euro = Convert.ToDecimal(xmlVerisi.SelectSingleNode(string.Format("Tarih_Date/Currency[@Kod='{0}']/ForexSelling", "EUR")).InnerText.Replace('.', ','));
            sterlin = Convert.ToDecimal(xmlVerisi.SelectSingleNode(string.Format("Tarih_Date/Currency[@Kod='{0}']/ForexSelling", "GBP")).InnerText.Replace('.', ','));
    
            if (birim == "Türk Lirası")
            {
                deger = 10000;
                return deger;
            }
            else if (birim == "ABD Doları")
            {
                deger = dolar;
                return deger;
            }
            else if (birim == "Euro")
            {
                deger = euro;
                return deger;
            }
            else 
            {
                deger = sterlin;
                return deger;
            }
           
        }
    }
}
