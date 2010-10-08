using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.Options;

namespace Tests.Unit_Tests
{
    using System;

    [TestClass]
    public class OptionParsingUnitTests
    {
        private static readonly OptionDefinition[] SingleRequiredArgument = new[]
        {
            new OptionDefinition { LongName = "aardvark", ShortName = 'a', Argument = OptionArgument.Required },
        };

        private static readonly OptionDefinition[] SingleOptionalArgument = new[]
        {
            new OptionDefinition { LongName = "aardvark", ShortName = 'a', Argument = OptionArgument.Optional },
        };

        private static readonly OptionDefinition[] SingleNoneArgument = new[]
        {
            new OptionDefinition { LongName = "aardvark", ShortName = 'a' },
        };

        private static readonly OptionDefinition[] NoneOptionalRequiredArguments = new[]
        {
            new OptionDefinition { LongName = "aaa", ShortName = 'a', Argument = OptionArgument.None },
            new OptionDefinition { LongName = "bbb", ShortName = 'b', Argument = OptionArgument.Optional },
            new OptionDefinition { LongName = "ccc", ShortName = 'c', Argument = OptionArgument.Required },
        };

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void ShortOptionRun_WithFirstOptionUnknown_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-na" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void ShortOptionRun_WithSecondOptionUnknown_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-an" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionRun_WithFirstOptionRequiringArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = NoneOptionalRequiredArguments };
            var results = parser.Parse(new[] { "-ca" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionRun_WithSecondOptionRequiringArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = NoneOptionalRequiredArguments };
            var results = parser.Parse(new[] { "-ac" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void ShortOptionRun_WithColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-aardvark:animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void ShortOptionRun_WithEmptyColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-aardvark:" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void ShortOptionRun_WithEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-aardvark=animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void ShortOptionRun_WithEmptyEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-aardvark=" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongOptionNoneArgument_WithColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "--aardvark:animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongOptionNoneArgument_WithEmptyColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "--aardvark:" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongOptionNoneArgument_WithEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "--aardvark=animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongOptionNoneArgument_WithEmptyEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "--aardvark=" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionNoneArgument_WithColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-a:animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionNoneArgument_WithEmptyColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-a:" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionNoneArgument_WithEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-a=animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionNoneArgument_WithEmptyEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-a=" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongSlashOptionNoneArgument_WithColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/aardvark:animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongSlashOptionNoneArgument_WithEmptyColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/aardvark:" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongSlashOptionNoneArgument_WithEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/aardvark=animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongSlashOptionNoneArgument_WithEmptyEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/aardvark=" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortSlashOptionNoneArgument_WithColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/a:animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortSlashOptionNoneArgument_WithEmptyColonArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/a:" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortSlashOptionNoneArgument_WithEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/a=animal" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortSlashOptionNoneArgument_WithEmptyEqualsArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/a=" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongOptionRequiringArgument_WithoutArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortOptionRequiringArgument_WithoutArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void LongSlashOptionRequiringArgument_WithoutArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.OptionArgumentException))]
        public void ShortSlashOptionRequiringArgument_WithoutArgument_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void UnknownShortOption_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-n" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void UnknownShortSlashOption_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/n" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void UnknownLongOption_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--n" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.UnknownOptionException))]
        public void UnknownLongSlashOption_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/long" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void JustADash_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-", "a" }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(OptionParsingException.InvalidParameterException))]
        public void JustASlash_ThrowsException()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/", "a" }).ToArray();
        }

        [TestMethod]
        public void LongSlashOptionRequiringArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionRequiringArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionRequiringArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionRequiringArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionRequiringArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/aardvark=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionRequiringArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionRequiringArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionRequiringArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionRequiringArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionRequiringArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "/a=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionOptionalArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionOptionalArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/aardvark:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionOptionalArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/aardvark:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionOptionalArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/aardvark=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionOptionalArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/aardvark=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionOptionalArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionOptionalArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/a:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionOptionalArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/a:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionOptionalArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/a=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionOptionalArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "/a=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionRequiringArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionRequiringArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionRequiringArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionRequiringArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionRequiringArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "--aardvark=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionRequiringArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionRequiringArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionRequiringArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionRequiringArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionRequiringArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleRequiredArgument };
            var results = parser.Parse(new[] { "-a=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleRequiredArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a:animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithEmptyColonArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a:" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a=animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithEmptyEqualsArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a=" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = string.Empty },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionAfterOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark", "--aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionAfterOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark", "-a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionAfterOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark", "/aardvark", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionAfterOptionalArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--aardvark", "/a", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0], Argument = "animal" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongOptionNoneArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "--aardvark" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleNoneArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void LongSlashOptionNoneArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/aardvark" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleNoneArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortOptionNoneArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "-a" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleNoneArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortSlashOptionNoneArgument_WithoutArgument_Parses()
        {
            var parser = new OptionParser { Definitions = SingleNoneArgument };
            var results = parser.Parse(new[] { "/a" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleNoneArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void PositionalArguments_Parse()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "positional1", "positional2" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Argument = "positional1" },
                new OptionParser.Option { Argument = "positional2" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void PositionalArguments_AfterEndOfOptionsMarker_Parse()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "--", "positional1", "positional2" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Argument = "positional1" },
                new OptionParser.Option { Argument = "positional2" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void PositionalArguments_AfterOptionWithOptionalArgument_Parse()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-a", "--", "positional1", "positional2" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Argument = "positional1" },
                new OptionParser.Option { Argument = "positional2" },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortRunOptionalArguments_Parses()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-aaa" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
            }, new OptionComparer()));
        }

        [TestMethod]
        public void ShortRunOptionalArguments_WithArgumet_IsParsedAsPositionalArgument()
        {
            var parser = new OptionParser { Definitions = SingleOptionalArgument };
            var results = parser.Parse(new[] { "-aaa", "animal" }).ToArray();
            Assert.IsTrue(results.SequenceEqual(new[]
            {
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Definition = SingleOptionalArgument[0] },
                new OptionParser.Option { Argument = "animal" },
            }, new OptionComparer()));
        }

        private sealed class OptionComparer : IEqualityComparer<OptionParser.Option>
        {
            public bool Equals(OptionParser.Option x, OptionParser.Option y)
            {
                return (x.Definition == y.Definition && x.Argument == y.Argument);
            }

            public int GetHashCode(OptionParser.Option obj)
            {
                return (obj.Definition.GetHashCode() ^ obj.Argument.GetHashCode());
            }
        }

        private sealed class SimpleParameters : OptionArgumentsBase
        {
            [Option("Animal", 'a')]
            public string Animal { get; set; }

            [Option("Flag", 'f', OptionArgument.None)]
            public bool Flag { get; set; }

            [PositionalArgument(0)]
            public int FirstPositionalArgument { get; set; }

            [PositionalArgument(1)]
            public double? SecondPositionalArgument { get; set; }

            [PositionalArgument(2)]
            public Guid? ThirdPositionalArgument { get; set; }

            public override void Validate()
            {
            }
        }

        [TestMethod]
        public void SimpleParameters_WithSingleParameter_IsParsed()
        {
            var parameters = new SimpleParameters();
            OptionParser.Parse(new[] { "--Animal", "horse" }, parameters);
            Assert.AreEqual("horse", parameters.Animal);
        }

        [TestMethod]
        public void SimpleParameters_WithParsedParameter_IsParsed()
        {
            var parameters = new SimpleParameters();
            OptionParser.Parse(new[] { "13" }, parameters);
            Assert.AreEqual(13, parameters.FirstPositionalArgument);
        }

        [TestMethod]
        public void SimpleParameters_WithoutParameter_IsParsed()
        {
            var parameters = new SimpleParameters();
            OptionParser.Parse(new[] { "/f" }, parameters);
            Assert.IsTrue(parameters.Flag);
        }

        [TestMethod]
        public void SimpleParameters_WithFullParameters_IsParsed()
        {
            var parameters = new SimpleParameters();
            Guid test = Guid.NewGuid();
            OptionParser.Parse(new[] { "/f", "--Animal=horse", "--", "13", "14.5e09", test.ToString(), "overflow 1", "overflow 2" }, parameters);
            Assert.IsTrue(parameters.Flag);
            Assert.AreEqual("horse", parameters.Animal);
            Assert.AreEqual(13, parameters.FirstPositionalArgument);
            Assert.AreEqual(14.5e09, parameters.SecondPositionalArgument);
            Assert.AreEqual(test, parameters.ThirdPositionalArgument);
            Assert.AreEqual(2, parameters.AdditionalArguments.Count);
            Assert.AreEqual("overflow 1", parameters.AdditionalArguments[0]);
            Assert.AreEqual("overflow 2", parameters.AdditionalArguments[1]);
        }
    }
}
