//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="WhenParsingFeatureFiles.cs" company="PicklesDoc">
//  Copyright 2011 Jeffrey Cameron
//  Copyright 2012-present PicklesDoc team and community contributors
//
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

using System.Linq;

using Autofac;
using DocumentFormat.OpenXml.Bibliography;
using NFluent;
using NUnit.Framework;
using PicklesDoc.Pickles.Extensions;
using PicklesDoc.Pickles.ObjectModel;
using StringReader = System.IO.StringReader;

namespace PicklesDoc.Pickles.Test
{
    [TestFixture]
    public class WhenParsingFeatureFiles : BaseFixture
    {
        private const string DocStringDelimiter = "\"\"\"";

        [Test]
        public void ThenCanParseFeatureWithMultipleScenariosSuccessfully()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    Scenario: Another scenario
    Given some other feature
    When it runs
    Then I should see that this other thing happens
        And something else";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature).IsNotNull();
            Check.That(feature.Name).IsEqualTo("Test");
            Check.That(feature.Description.ComparisonNormalize()).IsEqualTo(@"In order to do something
As a user
I want to run this scenario".ComparisonNormalize());
            Check.That(feature.FeatureElements.Count).IsEqualTo(2);
            Check.That(feature.Tags).IsEmpty();

            IFeatureElement scenario = feature.FeatureElements[0];
            Check.That(scenario.Name).IsEqualTo("A scenario");
            Check.That(scenario.Description).IsEqualTo("");
            Check.That(scenario.Steps.Count).IsEqualTo(3);
            Check.That(scenario.Tags).IsEmpty();

            Step givenStep = scenario.Steps[0];
            Check.That(givenStep.Keyword).IsEqualTo(Keyword.Given);
            Check.That(givenStep.Name).IsEqualTo("some feature");
            Check.That(givenStep.DocStringArgument).IsNull();
            Check.That(givenStep.TableArgument).IsNull();

            Step whenStep = scenario.Steps[1];
            Check.That(whenStep.Keyword).IsEqualTo(Keyword.When);
            Check.That(whenStep.Name).IsEqualTo("it runs");
            Check.That(whenStep.DocStringArgument).IsNull();
            Check.That(whenStep.TableArgument).IsNull();

            Step thenStep = scenario.Steps[2];
            Check.That(thenStep.Keyword).IsEqualTo(Keyword.Then);
            Check.That(thenStep.Name).IsEqualTo("I should see that this thing happens");
            Check.That(thenStep.DocStringArgument).IsNull();
            Check.That(thenStep.TableArgument).IsNull();

            IFeatureElement scenario2 = feature.FeatureElements[1];
            Check.That(scenario2.Name).IsEqualTo("Another scenario");
            Check.That(scenario2.Description).IsEqualTo(string.Empty);
            Check.That(scenario2.Steps.Count).IsEqualTo(4);
            Check.That(scenario2.Tags).IsEmpty();

            Step givenStep2 = scenario2.Steps[0];
            Check.That(givenStep2.Keyword).IsEqualTo(Keyword.Given);
            Check.That(givenStep2.Name).IsEqualTo("some other feature");
            Check.That(givenStep2.DocStringArgument).IsNull();
            Check.That(givenStep2.TableArgument).IsNull();

            Step whenStep2 = scenario2.Steps[1];
            Check.That(whenStep2.Keyword).IsEqualTo(Keyword.When);
            Check.That(whenStep2.Name).IsEqualTo("it runs");
            Check.That(whenStep2.DocStringArgument).IsNull();
            Check.That(whenStep2.TableArgument).IsNull();

            Step thenStep2 = scenario2.Steps[2];
            Check.That(thenStep2.Keyword).IsEqualTo(Keyword.Then);
            Check.That(thenStep2.Name).IsEqualTo("I should see that this other thing happens");
            Check.That(thenStep2.DocStringArgument).IsNull();
            Check.That(thenStep2.TableArgument).IsNull();

            Step thenStep3 = scenario2.Steps[3];
            Check.That(thenStep3.Keyword).IsEqualTo(Keyword.And);
            Check.That(thenStep3.Name).IsEqualTo("something else");
            Check.That(thenStep3.DocStringArgument).IsNull();
            Check.That(thenStep3.TableArgument).IsNull();
        }

        [Test]
        public void ThenCanParseMostBasicFeatureSuccessfully()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature).IsNotNull();
            Check.That(feature.Name).IsEqualTo("Test");
            Check.That(feature.Description.ComparisonNormalize()).IsEqualTo(@"In order to do something
As a user
I want to run this scenario".ComparisonNormalize());
            Check.That(feature.FeatureElements.Count).IsEqualTo(1);
            Check.That(feature.Tags).IsEmpty();

            IFeatureElement scenario = feature.FeatureElements.First();
            Check.That(scenario.Name).IsEqualTo("A scenario");
            Check.That(scenario.Description).IsEqualTo(string.Empty);
            Check.That(scenario.Steps.Count).IsEqualTo(3);
            Check.That(scenario.Tags).IsEmpty();

            Step givenStep = scenario.Steps[0];
            Check.That(givenStep.Keyword).IsEqualTo(Keyword.Given);
            Check.That(givenStep.Name).IsEqualTo("some feature");
            Check.That(givenStep.DocStringArgument).IsNull();
            Check.That(givenStep.TableArgument).IsNull();

            Step whenStep = scenario.Steps[1];
            Check.That(whenStep.Keyword).IsEqualTo(Keyword.When);
            Check.That(whenStep.Name).IsEqualTo("it runs");
            Check.That(whenStep.DocStringArgument).IsNull();
            Check.That(whenStep.TableArgument).IsNull();

            Step thenStep = scenario.Steps[2];
            Check.That(thenStep.Keyword).IsEqualTo(Keyword.Then);
            Check.That(thenStep.Name).IsEqualTo("I should see that this thing happens");
            Check.That(thenStep.DocStringArgument).IsNull();
            Check.That(thenStep.TableArgument).IsNull();
        }

        [Test]
        public void ThenCanParseScenarioOutlinesSuccessfully()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

  Scenario Outline: A scenario outline
    Given some feature with <keyword1>
    When it runs
    Then I should see <keyword2>

    Examples:
    | keyword1 | keyword2 |
    | this     | that     |";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            var scenarioOutline = feature.FeatureElements[0] as ScenarioOutline;
            Check.That(scenarioOutline).IsNotNull();
            Check.That(scenarioOutline.Name).IsEqualTo("A scenario outline");
            Check.That(scenarioOutline.Description).IsEqualTo(string.Empty);
            Check.That(scenarioOutline.Steps.Count).IsEqualTo(3);

            Step givenStep = scenarioOutline.Steps[0];
            Check.That(givenStep.Keyword).IsEqualTo(Keyword.Given);
            Check.That(givenStep.Name).IsEqualTo("some feature with <keyword1>");
            Check.That(givenStep.DocStringArgument).IsNull();
            Check.That(givenStep.TableArgument).IsNull();

            Step whenStep = scenarioOutline.Steps[1];
            Check.That(whenStep.Keyword).IsEqualTo(Keyword.When);
            Check.That(whenStep.Name).IsEqualTo("it runs");
            Check.That(whenStep.DocStringArgument).IsNull();
            Check.That(whenStep.TableArgument).IsNull();

            Step thenStep = scenarioOutline.Steps[2];
            Check.That(thenStep.Keyword).IsEqualTo(Keyword.Then);
            Check.That(thenStep.Name).IsEqualTo("I should see <keyword2>");
            Check.That(thenStep.DocStringArgument).IsNull();
            Check.That(thenStep.TableArgument).IsNull();

            var examples = scenarioOutline.Examples;
            Check.That(examples.First().Name).IsNullOrEmpty();
            Check.That(examples.First().Description).IsNullOrEmpty();

            Table table = examples.First().TableArgument;
            Check.That(table.HeaderRow.Cells[0]).IsEqualTo("keyword1");
            Check.That(table.HeaderRow.Cells[1]).IsEqualTo("keyword2");
            Check.That(table.DataRows[0].Cells[0]).IsEqualTo("this");
            Check.That(table.DataRows[0].Cells[1]).IsEqualTo("that");
        }

        [Test]
        public void ThenCanParseScenarioWithBackgroundSuccessfully()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    Background: Some background for the scenarios
    Given some prior context
        And yet more prior context

  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.Background).IsNotNull();
            Check.That(feature.Background.Name).IsEqualTo("Some background for the scenarios");
            Check.That(feature.Background.Description).IsEqualTo(string.Empty);
            Check.That(feature.Background.Steps.Count).IsEqualTo(2);
            Check.That(feature.Background.Tags).IsEmpty();

            Step givenStep1 = feature.Background.Steps[0];
            Check.That(givenStep1.Keyword).IsEqualTo(Keyword.Given);
            Check.That(givenStep1.Name).IsEqualTo("some prior context");
            Check.That(givenStep1.DocStringArgument).IsNull();
            Check.That(givenStep1.TableArgument).IsNull();

            Step givenStep2 = feature.Background.Steps[1];
            Check.That(givenStep2.Keyword).IsEqualTo(Keyword.And);
            Check.That(givenStep2.Name).IsEqualTo("yet more prior context");
            Check.That(givenStep2.DocStringArgument).IsNull();
            Check.That(givenStep2.TableArgument).IsNull();
        }

        [Test]
        public void ThenCanParseScenarioWithDocstringSuccessfully()
        {
            string docstring = string.Format(
                @"{0}
This is a document string
it can be many lines long
{0}",
                DocStringDelimiter);

            string featureText =
                string.Format(
                    @"Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    Scenario: A scenario
        Given some feature
        {0}
        When it runs
        Then I should see that this thing happens",
                    docstring);

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.FeatureElements[0].Steps[0].DocStringArgument.ComparisonNormalize()).IsEqualTo(@"This is a document string
it can be many lines long".ComparisonNormalize());
        }

        [Test]
        public void ThenCanParseScenarioWithTableSuccessfully()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

  Scenario: A scenario
    Given some feature with a table
        | Column1 | Column2 |
        | Value 1 | Value 2 |
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Table table = feature.FeatureElements[0].Steps[0].TableArgument;
            Check.That(table.HeaderRow.Cells[0]).IsEqualTo("Column1");
            Check.That(table.HeaderRow.Cells[1]).IsEqualTo("Column2");
            Check.That(table.DataRows[0].Cells[0]).IsEqualTo("Value 1");
            Check.That(table.DataRows[0].Cells[1]).IsEqualTo("Value 2");
        }

        [Test]
        public void Then_can_parse_scenario_with_tags_successfully()
        {
            string featureText =
                @"# ignore this comment
@feature-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.Tags[0]).IsEqualTo("@feature-tag");
            Check.That(feature.FeatureElements[0].Tags[0]).IsEqualTo("@scenario-tag-1");
            Check.That(feature.FeatureElements[0].Tags[1]).IsEqualTo("@scenario-tag-2");
        }

//26.04.00 MI:add RCIS theme and story tag parsing from feature comments rapoe01 02/25/2019 - 
        [Test]
        public void Parse_story_and_theme_from_RCIS_comment()
        {
            string featureText =
                @"# ignore this comment
Feature: CLU
	Validate CLU entry
#24.01.00 RI:102069-03 (B-12010) heilr01 08/19/2016 - enter specflow
@Integration
Scenario: Verify_CLU_Against_CLU_Certified
#24.01.00 RI:B-12010 heilr01 08/19/2016 - Display hard stop when CLU entered does not exist in CLU table for the given State/County
	Given I have entered FN ""271"", Tract ""4679"", Field ""4""
            When I press OK
            Then the number of CLUObjects is 1
            And The CLU list should contain
                | CLUID | FSAAdminStateID | FSAAdminCountyID | LocationStateID | LocationCountyID |
                | 610548e2 - 9a4d - 11d6 - 935c - 00c04f5c086f | 27 | 153 | 27 | 153 |
#24.04.00 RI:9026-101 (B-15726) EGALR01- 3/17/2017 G8: ARPI - Mapping - ACRSI Send -Populate the FSA values correctly when we have a great8 crop (CLU grid and Mapping)
# this test is to check the dropdown list of FSA Commodity,Type,IntendedUse 
                @Scheduled
            Scenario: Check FSA values For G8 Crop
            Given I load Policy Crop Year 2017, Policy Number ""861621"", State ""OH""
            And I have loaded an existing ARPolicy of Crop Year 2017, Policy Number ""861621"", State ""OH""
            And I load the following Crop Line: ""WHEAT|ARP|NTS"" , ""Policy"" , ""2017,OH,861621""
            And Agency of loaded policy has ACRSI Send Set
                When I set typecode classcode subclasscode for PolicyCrop ""PolicyCrop"", ""WHEAT|ARP|NTS"" to ""997"", ""091"", ""997""
            Then The FSACommodity list should contain
                | Commodity |
                | WHEAT |
                And The FSA Type list should contain
                | FSA Type |
                | Hard Amber Durum, Winter |
                | Hard Red, Winter |
                | Hard White, Winter |
                | Soft Red, Winter |
                | soft White, Winter |
            And The FSAIntendedUse list should contain
                | FSA Intended Use |
                | Grazing |
                | Seed |
                | Green Manure |
                | Grain |
                | Cover |
                | Haying |
                | Green Chop |
                | Processed |";
            var parser = Container.Resolve<FeatureParser>();
            Configuration.commentParsing = "RCIS.CIMax";
            Configuration.DisableComments();
            Feature feature = parser.Parse(new StringReader(featureText));

            IFeatureElement scenario = feature.FeatureElements.First();
            Check.That(scenario.Tags).Contains("@Integration");
            Step stepGiven = scenario.Steps[0];
            // esr 2/26/2019 translate hyphen in theme number to underscore
            Check.That(scenario.Tags).Contains("@T_102069_03");
            Check.That(scenario.Tags).Contains("@B_12010");

            Check.That(scenario.Steps.Count).IsEqualTo(4);
            Check.That(feature.FeatureElements.Count).IsEqualTo(2);

            scenario = feature.FeatureElements[1];
            Check.That(scenario.Tags).Contains("@T_9026_101");
            Check.That(scenario.Tags).Contains("@B_15726");
        }
        // esr 2/26/2019 don't add a story tag if story tag exists in old format without underscore
        [Test]
        public void dont_Parse_story_from_RCIS_comment_where_old_tag_exists()
        {
            string featureText =
                @"# ignore this comment
Feature: CLU
	Validate CLU entry
#24.01.00 RI:102069-03 (B-12010) heilr01 08/19/2016 - enter specflow
@Integration
@B12010
Scenario: test
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Configuration.commentParsing = "RCIS.CIMax";
            Configuration.DisableComments();
            Feature feature = parser.Parse(new StringReader(featureText));

            IFeatureElement scenario = feature.FeatureElements.First();
            Check.That(scenario.Tags).ContainsOnlyElementsThatMatch(t=>(t=="@Integration"||t=="@B12010"||t=="@T_102069_03"));
        }

        [Test]
        public void Then_can_parse_scenario_with_comments_successfully()
        {
            string featureText =
              @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

  Scenario: A scenario
    # A single line comment
    Given some feature
    # A multiline comment - first line
    # Second line
    When it runs
    Then I should see that this thing happens
    # A last comment after the scenario";

            var parser = Container.Resolve<FeatureParser>();
            Feature feature = parser.Parse(new StringReader(featureText));

            IFeatureElement scenario = feature.FeatureElements.First();

            Step stepGiven = scenario.Steps[0];
            Check.That(stepGiven.Comments.Count).IsEqualTo(1);
            Check.That(stepGiven.Comments[0].Text).IsEqualTo("# A single line comment");

            Step stepWhen = scenario.Steps[1];
            Check.That(stepWhen.Comments.Count).IsEqualTo(2);
            Check.That(stepWhen.Comments[0].Text).IsEqualTo("# A multiline comment - first line");
            Check.That(stepWhen.Comments[1].Text).IsEqualTo("# Second line");

            Step stepThen = scenario.Steps[2];
            Check.That(stepThen.Comments.Count).IsEqualTo(1);
            Check.That(stepThen.Comments.Count(o => o.Type == CommentType.StepComment)).IsEqualTo(0);
            Check.That(stepThen.Comments.Count(o => o.Type == CommentType.AfterLastStepComment)).IsEqualTo(1);
            Check.That(stepThen.Comments[0].Text = "# A last comment after the scenario");
        }



        [Test]
        public void Then_can_parse_and_ignore_feature_with_tag_in_configuration_ignore_tag()
        {
            var featureText =
                @"# ignore this comment
@feature-tag @exclude-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));
            Check.That(feature).IsNull();
        }

        [Test]
        public void Then_can_parse_and_remove_technical_tag_in_configuration_remove_technical_tag()
        {
            var featureText =
                @"# ignore this comment
@feature-tag @TagsToHideFeature
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));
            Check.That(feature).IsNotNull();

            Check.That(feature.Tags).ContainsExactly("@feature-tag");
        }

        [Test]
        public void Then_can_parse_and_ignore_scenario_with_tag_in_configuration_ignore_tag()
        {
            var featureText =
                @"# ignore this comment
@feature-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2 @exclude-tag
  Scenario: B scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2
  Scenario: C scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";
 
            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.FeatureElements.Count).IsEqualTo(2);
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "A scenario")).IsNotNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "B scenario")).IsNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "C scenario")).IsNotNull();
        }

        [Test]
        public void Then_can_parse_and_remove_tag_in_configuration_remove_technical_tag_from_scenario()
        {
            var featureText =
                @"# ignore this comment
@feature-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2 @TagsToHideScenario
  Scenario: B scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2
  Scenario: C scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.FeatureElements.Count).IsEqualTo(3);
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "A scenario")).IsNotNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "B scenario")).IsNotNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "B scenario").Tags).ContainsExactly("@scenario-tag-1", "@scenario-tag-2");
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "C scenario")).IsNotNull();
        }

        [Test]
        public void Then_can_parse_and_ignore_scenario_with_tag_in_configuration_ignore_tag_and_do_not_keep_feature()
        {
            var featureText =
                @"# ignore this comment
@feature-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2 @Exclude-Tag
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2 @exclude-tag
  Scenario: B scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));

            Check.That(feature).IsNull();
        }

        [Test]
        public void Then_can_parse_and_ignore_with_with_tag_without_sensitivity()
        {

            var featureText =
                @"# ignore this comment
@feature-tag
Feature: Test
    In order to do something
    As a user
    I want to run this scenario

    @scenario-tag-1 @scenario-tag-2 @Exclude-Tag
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2 @exclude-tag
  Scenario: B scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2 @ExClUdE-tAg
  Scenario: C scenario
    Given some feature
    When it runs
    Then I should see that this thing happens

    @scenario-tag-1 @scenario-tag-2
  Scenario: D scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            var feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.FeatureElements.Count).IsEqualTo(1);
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "A scenario")).IsNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "B scenario")).IsNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "C scenario")).IsNull();
            Check.That(feature.FeatureElements.FirstOrDefault(fe => fe.Name == "D scenario")).IsNotNull();
        }
    }
}
    