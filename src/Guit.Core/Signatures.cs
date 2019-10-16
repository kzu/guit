using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit
{
    public class Signatures
    {
        public static Signature GetStashSignature() => new Signature("Guit-Stash", "stash@guit.com", DateTimeOffset.Now);
    }
}