using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace GetAdDecodedPassword
{
    class Program
    {
        static void Main(string[] args)
        {
            var interestingAttributeNames = new List<string> { "UnixUserPassword", "UserPassword", 
                "unicodePwd", "msSFU30Name", "msSFU30Password", "os400-password" };
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                            var passAttributes = new Dictionary<string, string>();
                            foreach (string property in interestingAttributeNames)
                            {
                                var val = de.Properties[property]?.Value;
                                if (val != null)
                                {
                                    passAttributes.Add(property, val.GetType().IsArray ? Encoding.UTF8.GetString((byte[])val) : val.ToString());
                                }
                            }

                            if (passAttributes.Count > 0)
                            {
                                Console.WriteLine("Username: " + de.Properties["samaccountname"]?.Value);
                                Console.WriteLine(string.Join(Environment.NewLine, passAttributes.Select(kvp => kvp.Key + ": " + kvp.Value)));
                                Console.WriteLine();
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
