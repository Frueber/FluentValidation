﻿#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FluentValidation.Tests {
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Xunit;

	public class TransformTests {
		[Fact]
		public void Transforms_property_value() {
			var validator = new InlineValidator<Person>();
			validator.RuleFor(x => x.Surname).Transform(name => "foo" + name).Equal("foobar");

			var result = validator.Validate(new Person {Surname = "bar"});
			result.IsValid.ShouldBeTrue();
		}

		[Fact]
		public void Transforms_property_value_to_another_type() {
			var validator = new InlineValidator<Person>();
			validator.RuleFor(x => x.Surname).Transform(name => 1).GreaterThan(10);

			var result = validator.Validate(new Person {Surname = "bar"});
			result.IsValid.ShouldBeFalse();
			result.Errors[0].ErrorCode.ShouldEqual("GreaterThanValidator");
		}

		[Fact]
		public void Transforms_property_value_with_propagated_original_object() {
			var validator = new InlineValidator<Person>();
			validator.RuleFor(x => x.Forename)
				.Transform((person, forename) => new {Nicks = person.NickNames, Name = forename})
				.Must(context => context.Nicks.Any(nick => nick == context.Name.ToLower()));

			var result = validator.Validate(new Person {NickNames = new[] {"good11", "peter"}, Forename = "Peter"});
			result.IsValid.ShouldBeTrue();
		}

		[Fact]
		public async Task Transforms_property_value_with_propagated_original_object_async() {
			var validator = new InlineValidator<Person>();
			validator.RuleFor(x => x.Forename)
				.Transform((person, forename) => new {Nicks = person.NickNames, Name = forename})
				.Must(context => context.Nicks.Any(nick => nick == context.Name.ToLower()));

			var result = await validator.ValidateAsync(new Person {NickNames = new[] {"good11", "peter"}, Forename = "Peter"});
			result.IsValid.ShouldBeTrue();
		}

		[Fact]
		public void Transforms_collection_element() {
			var validator = new InlineValidator<Person>();
			validator.RuleForEach(x => x.Orders)
				.Transform(order => order.Amount)
				.GreaterThan(0);

			var result = validator.Validate(new Person() {Orders = new List<Order> {new Order()}});
			result.Errors.Count.ShouldEqual(1);
		}

		[Fact]
		public async Task Transforms_collection_element_async() {
			var validator = new InlineValidator<Person>();
			validator.RuleForEach(x => x.Orders)
				.Transform(order => order.Amount)
				.MustAsync((amt, token) => Task.FromResult(amt > 0));

			var result = await validator.ValidateAsync(new Person() {Orders = new List<Order> {new Order()}});
			result.Errors.Count.ShouldEqual(1);
		}

		public void Transforms_collection_element_with_propagated_original_object() {
			var validator = new InlineValidator<Person>();
			validator.RuleForEach(x => x.Children)
				.Transform((parent, children) => new {ParentName = parent.Surname, Children = children})
				.Must(context => context.ParentName == context.Children.Surname);

			var child = new Person {Surname = "Pupa"};
			var result = validator.Validate(new Person() {Surname = "Lupa", Children = new List<Person> {child}});
			result.IsValid.ShouldBeFalse();
			result.Errors.Count.ShouldEqual(1);
		}

		[Fact]
		public async Task Transforms_collection_element_with_propagated_original_object_async() {
			var validator = new InlineValidator<Person>();
			validator.RuleForEach(x => x.Children)
				.Transform((parent, children) => new {ParentName = parent.Surname, Children = children})
				.Must(context => context.ParentName == context.Children.Surname);

			var child = new Person {Surname = "Pupa"};
			var result = await validator.ValidateAsync(new Person() {Surname = "Lupa", Children = new List<Person> {child}});
			result.IsValid.ShouldBeFalse();
			result.Errors.Count.ShouldEqual(1);
		}
	}
}
