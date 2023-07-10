﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer.Walkers;

public interface ISyntaxWalker
{
    public ICollection<SyntaxNodeContainer> SyntaxNodes { get; }
    //public ICollection<CSharpSyntaxNode> ParameterNodes { get; }
    public List<string> Log { get; }
}
