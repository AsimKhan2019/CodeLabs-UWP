using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Microsoft.Labs.SightsToSee.Library.Models;

namespace Microsoft.Labs.SightsToSee.Common
{
    internal class AccountsHelper
    {
        private const string USER_LIST_FILE_NAME = "accountlist.txt";

        /// <summary>
        ///     Gets the account list file and deserializes it from XML to a list of accounts object.
        /// </summary>
        /// <returns>List of account objects</returns>
        public static async Task<List<Account>> LoadAccountList()
        {
            try
            {
                var accountsFile = await ApplicationData.Current.LocalFolder.GetFileAsync(USER_LIST_FILE_NAME);
                var accountsXml = await FileIO.ReadTextAsync(accountsFile);
                return DeserializeXmlToAccountList(accountsXml);
            }
            catch (FileNotFoundException)
            {
                var emptyAccountList = new List<Account>();
                return emptyAccountList;
            }
        }

        /// <summary>
        ///     Takes a list of accounts and create an account list file. (Replacing the old one)
        /// </summary>
        /// <param name="accountList">List object of accounts</param>
        public static async void SaveAccountList(List<Account> accountList)
        {
            var accountsXml = SerializeAccountListToXml(accountList);

            try
            {
                var accountsFile = await ApplicationData.Current.LocalFolder.GetFileAsync(USER_LIST_FILE_NAME);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }
            catch (FileNotFoundException)
            {
                var accountsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(USER_LIST_FILE_NAME);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }
        }

        /// <summary>
        ///     Takes a list of accounts and returns an XML formatted string representing the list
        /// </summary>
        /// <param name="list">List object of accounts</param>
        /// <returns>XML formatted list of accounts</returns>
        public static string SerializeAccountListToXml(List<Account> list)
        {
            var xmlizer = new XmlSerializer(typeof (List<Account>));
            var writer = new StringWriter();
            xmlizer.Serialize(writer, list);
            return writer.ToString();
        }

        /// <summary>
        ///     Takes an XML formatted string representing a list of accounts and returns a list object of accounts
        /// </summary>
        /// <param name="listAsXml">XML formatted list of accounts</param>
        /// <returns>List object of accounts</returns>
        public static List<Account> DeserializeXmlToAccountList(string listAsXml)
        {
            var xmlizer = new XmlSerializer(typeof (List<Account>));
            var accounts = new List<Account>();
            TextReader textreader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(listAsXml)));
            accounts = xmlizer.Deserialize(textreader) as List<Account>;
            return accounts;
        }
    }
}