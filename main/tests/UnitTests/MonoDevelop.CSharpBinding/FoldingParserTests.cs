// 
// FoldingParserTests.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using NUnit.Framework;

using MonoDevelop.CSharp.Parser;
using Mono.TextEditor;
using MonoDevelop.Projects.Dom;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.CSharpBinding.Tests
{
	[TestFixture]
	public class FoldingParserTests
	{
		static void Test (string code)
		{
			var parser = new CSharpFoldingParser ();
			var sb = new StringBuilder ();
			var openStack = new Stack<DomLocation>();
			
			int line = 1;
			int col = 1;
			
			var foldingList = new List<DomRegion> ();
			
			for (int i = 0; i < code.Length; i++) {
				char ch = code [i];
				switch (ch) {
				case '[':
					openStack.Push (new DomLocation (line, col));
					break;
				case ']':
					foldingList.Add (new DomRegion (openStack.Pop (), new DomLocation (line, col)));
					break;
				default:
					if (ch =='\n') {
						line++;
						col = 1;
					} else {
						col++;
					}
					sb.Append (ch);
					break;
				}
			}
			
			var doc = parser.Parse ("a.cs", sb.ToString ());
			var generatedFoldings = new List<FoldingRegion> (doc.GenerateFolds ());
			Assert.AreEqual (foldingList.Count, generatedFoldings.Count, "Folding count differs.");
			foreach (var generated in generatedFoldings) {
				Assert.IsTrue (foldingList.Any (f => f == generated.Region), "fold not found:" + generated.Region);
			}
		}
		
		[Test]
		public void TestMultiLineComment ()
		{
			Test (@"class Test {

} [/* 

Comment 

*/]

class SomeNew {


}");
		}
		
		[Test]
		public void TestSingleLineComment ()
		{
			Test (@"class Test {
	public static void Main (string args)
	{
		Something (); // Hello World
	}
");
		
		}
		
		[Test]
		public void TestFileHeader ()
		{
			Test (@"[// 
// EnumMemberDeclaration.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the ""Software""), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.]
using System;");
		}
		
		[Ignore("To be done.")]
		[Test]
		public void TestRegions ()
		{
			Test (@"class Test
{
	[#region TestRegion
	void FooBar ()
	{
	}
	#endregion]
}");
		}
		
	}
}
