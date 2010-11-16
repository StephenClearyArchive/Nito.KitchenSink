using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nito.KitchenSink;

namespace GuidCracker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Enter a GUID value (blank to exit): ");
                    var guidString = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(guidString))
                    {
                        return;
                    }

                    var guid = Guid.Parse(guidString);
                    var variant = guid.GetVariant();
                    Console.WriteLine("Variant: " + variant);
                    if (variant == GuidVariant.RFC4122)
                    {
                        var version = guid.GetVersion();
                        Console.WriteLine("Version: " + version);
                        if (version == GuidVersion.Random)
                        {
                            Console.WriteLine("Random bits: " + guid.GetRandom().PrettyDump());
                        }
                        else if (version == GuidVersion.NameBasedUsingMD5 || version == GuidVersion.NameBasedUsingSHA1)
                        {
                            Console.WriteLine("Hash bits: " + guid.GetHash());
                        }
                        else if (version == GuidVersion.TimeBased)
                        {
                            Console.WriteLine("Clock sequence: " + guid.GetClockSequence());
                            Console.WriteLine("Timestamp: " + new DateTimeOffset(guid.GetCreateTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff K"));
                            if (guid.NodeIsMAC())
                            {
                                Console.WriteLine("MAC address: " + guid.GetNode().PrettyDump());
                            }
                            else
                            {
                                Console.WriteLine("Random node identifier: " + guid.GetNode().PrettyDump());
                            }
                        }
                        else if (version == GuidVersion.DCESecurity)
                        {
                            Console.WriteLine("Clock sequence (partial): " + guid.GetPartialClockSequence());
                            Console.WriteLine("Timestamp (approximate): " + new DateTimeOffset(guid.GetPartialCreateTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fffffff K"));
                            if (guid.NodeIsMAC())
                            {
                                Console.WriteLine("MAC address: " + guid.GetNode().PrettyDump());
                            }
                            else
                            {
                                Console.WriteLine("Random node identifier: " + guid.GetNode().PrettyDump());
                            }

                            var domain = guid.GetDomain();
                            Console.WriteLine("Security domain: " + domain);
                            if (domain == DCEDomain.Person)
                            {
                                Console.WriteLine("POSIX UID: " + (uint)guid.GetLocalIdentifier());
                            }
                            else if (domain == DCEDomain.Group)
                            {
                                Console.WriteLine("POSIX GID: " + (uint)guid.GetLocalIdentifier());
                            }
                            else
                            {
                                Console.WriteLine("Local identifier: " + (uint)guid.GetLocalIdentifier());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
