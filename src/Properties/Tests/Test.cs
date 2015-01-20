//
//  Test.cs
//
//  Author:
//       Ross Jennings <themissinglinq@gmail.com>
//
//  Copyright (c) 2013 
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using CommentCleaner;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class ProgramTests
	{
		[Test]
		public void Main_When_Directory_Not_Specified_Throws_Exception()
		{
			string[] args = new string [] { };

			Assert.Throws<Exception>(() => Program.Main(args));
		}
	}
}
