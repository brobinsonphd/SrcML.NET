﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABB.SrcML.Data {
    public class NamespaceUse : NamedScopeUse {
        /// <summary>
        /// Creates a <see cref="NamespaceDefinition"/> object from this use (along with all of its descendants based on <see cref="ChildScopeUse"/>).
        /// </summary>
        /// <returns>A new namespace definition based on this use</returns>
        public override NamedScope CreateScope() {
            var ns = new NamespaceDefinition
                     {
                         Name = this.Name,
                         ProgrammingLanguage = this.ProgrammingLanguage
                     };
            ns.AddSourceLocation(this.Location);
            if(ChildScopeUse != null) {
                ns.AddChildScope(ChildScopeUse.CreateScope());
            }
            return ns;
        }

        
    }
}
