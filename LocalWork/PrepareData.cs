using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace KursyWalut
{
    public static class PrepareData
    {
        public static List<SelectedItems> elements = new List<SelectedItems>();
        public static List<string> selectedCurrencies_Names = new List<string>();
        public static List<string> selectedCurrencies_Values = new List<string>();
        public static List<bool> selectedCurrencies_Over = new List<bool>();
        public static DateTime updateDate;

        public static XmlDocument FormatXMLtoUTF8(string url)
        {
            var xml_table = new XmlDocument();
            try
            {
                xml_table.Load(url);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            XmlDeclaration xmlDeclaration = null;
            xmlDeclaration = xml_table.CreateXmlDeclaration("1.0", null, null);
            xmlDeclaration.Encoding = "utf-8";
            xml_table.ReplaceChild(xmlDeclaration, xml_table.FirstChild);

            Encoding iso = Encoding.GetEncoding(28592);
            Encoding utf8 = Encoding.UTF8;

            byte[] bytes = iso.GetBytes(xml_table.OuterXml); //xml_table.OuterXml

            byte[] convertISO = Encoding.Convert(iso, utf8, bytes);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Encoding.UTF8.GetString(convertISO));

            return xmlDoc;
        }

        public static DateTime GetDocumentDate(string url)
        {
            var doc = new XmlDocument();

            try
            {
                doc.Load(url);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            XmlNodeList dateNode = doc.SelectNodes("/tabela_kursow");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            string temp;
            XmlNode node = doc.SelectSingleNode("/tabela_kursow/data_publikacji");
            temp = node.InnerText;
            updateDate = Convert.ToDateTime(temp);
            return updateDate;
        }

        public static void XmlCurrencyNamesToList(XmlDocument xml_table)
        {
            XmlNodeList Node_tableA = xml_table.SelectNodes("/tabela_kursow/pozycja");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_table.NameTable);

            foreach (XmlNode node in Node_tableA)
            {
                elements.Add(new SelectedItems()
                {
                    CurrencyName = node.SelectSingleNode("nazwa_waluty", nsmgr).InnerText,
                    IsSelected = false,
                    ExchangeRate = node.SelectSingleNode("kurs_sredni", nsmgr).InnerText
                });
            }
        }

        public static List<string> XmlCurrencyNames(List<SelectedItems> elements)
        {
            var currencyNames = new List<string>();
            foreach (SelectedItems name in elements)
            {
                currencyNames.Add(name.CurrencyName);
            }
            return currencyNames;
        }
    }
}