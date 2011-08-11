// <copyright file="OptionArgumentsBase.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A base class for arguments classes, which use option attributes on their properties. Note that arguments classes do not <i>need</i> to derive from <see cref="OptionArgumentsBase"/>, but they do need to implement <see cref="IOptionArguments"/>.
    /// </summary>
    public abstract class OptionArgumentsBase : IOptionArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionArgumentsBase"/> class.
        /// </summary>
        protected OptionArgumentsBase()
        {
            this.AdditionalArguments = new List<string>();
        }

        /// <summary>
        /// Gets the list of additional positional arguments after those specified by <see cref="PositionalArgumentAttribute"/>.
        /// </summary>
        [PositionalArguments]
        public List<string> AdditionalArguments { get; private set; }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.AdditionalArguments != null);
        }

        /// <summary>
        /// Validates the arguments by throwing <see cref="OptionParsingException"/> errors as necessary. The <see cref="OptionArgumentsBase"/> implementation checks to ensure that <see cref="AdditionalArguments"/> is empty. Derived classes do not have to invoke the base method, if they wish to allow additional positional arguments.
        /// </summary>
        public virtual void Validate()
        {
            if (this.AdditionalArguments.Count != 0)
            {
                throw new OptionParsingException.UnknownOptionException("Unknown parameter  " + this.AdditionalArguments[0]);
            }
        }
    }
}
