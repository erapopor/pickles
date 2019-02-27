using System.Linq;

using Autofac;
using Autofac.Core;
using DocumentFormat.OpenXml.Bibliography;
using NFluent;
using NUnit.Framework;
using PicklesDoc.Pickles.Extensions;
using PicklesDoc.Pickles.ObjectModel;
using StringReader = System.IO.StringReader;

namespace PicklesDoc.Pickles.Test
{
    [TestFixture]
    class WhenParsingFeatureFileComments : BaseFixture
    {
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
            Check.That(scenario.Tags).ContainsOnlyElementsThatMatch(t => (t == "@Integration" || t == "@B12010" || t == "@T_102069_03"));
        }
        //esr 02/26/2019 don't parse RI:(B-12345) as theme
        [Test]
        public void dont_parse_story_preceded_by_RI_prefix()
        {

            string featureText =
                @"# ignore this comment
Feature: Test
#26.03.00 RI:(B-32045) mudis01 10/15/2018 - For 2018 Hail IPAReduced value should not be rounded
@Integration
@B_32045
@BR_HAIL163
Scenario: Round IPAReduced to Whole Dollar
	Given I load a hail Policy of Reinsurance Year 2018, Policy Number ""854310"", State ""IA""	                             
        And I have a hail line with following values
        | Acres | IPA | Prorata % |
        | 72    | 255 | .25        |
        When I Save
            Then the hail line will have the following calculated values:
        | Field         | Value   |
        | Liability     | 4590    |
";
            var parser = Container.Resolve<FeatureParser>();
            Configuration.commentParsing = "RCIS.CIMax";
            Configuration.DisableComments();
            Feature feature = parser.Parse(new StringReader(featureText));

            IFeatureElement scenario = feature.FeatureElements.First();
            Check.That(scenario.Tags).ContainsOnlyElementsThatMatch(t => (t == "@Integration" || t == "@B_32045" || t == "@BR_HAIL163"));

        }

        [Test]
        public void ThenCanSkipParsingCommentsBeforeBackground()
        {
            string featureText =
                @"# ignore this comment
Feature: Test
    In order to do something
    As a user
    I want to run this scenario
#RI:1234-56 (B-12345)
    Background: Some background for the scenarios
    Given some prior context
        And yet more prior context
#RI:5678-10 (B-23456)
  Scenario: A scenario
    Given some feature
    When it runs
    Then I should see that this thing happens";

            var parser = Container.Resolve<FeatureParser>();
            Configuration.commentParsing = "RCIS.CIMax";
            Configuration.DisableComments();
            Feature feature = parser.Parse(new StringReader(featureText));

            Check.That(feature.Background).IsNotNull();
        }
    }
}
