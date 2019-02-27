//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Mapper.cs" company="PicklesDoc">
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

using System;
using System.Collections.Generic;
using System.Linq;
//esr 02/26/2019 add theme and story tag parsing from feature comments - 
using System.Text.RegularExpressions;
using PicklesDoc.Pickles.Extensions;
using G = Gherkin.Ast;
using Match = System.Text.RegularExpressions.Match;

namespace PicklesDoc.Pickles.ObjectModel
{

    public class Mapper
    {
        private readonly IConfiguration configuration;

        private readonly ILanguageServices languageServices;

        public Mapper(IConfiguration configuration, ILanguageServices languageServices)
        {
            this.configuration = configuration;
            this.languageServices = languageServices;
        }

        public string MapToString(G.TableCell cell)
        {
            return cell?.Value;
        }

        public TableRow MapToTableRow(G.TableRow tableRow)
        {
            if (tableRow == null)
            {
                return null;
            }

            return new TableRow(tableRow.Cells.Select(this.MapToString));
        }

        public Table MapToTable(G.DataTable dataTable)
        {
            if (dataTable == null)
            {
                return null;
            }

            var tableRows = dataTable.Rows;
            return this.MapToTable(tableRows);
        }

        public Table MapToTable(IEnumerable<G.TableRow> tableRows)
        {
            return new Table
            {
                HeaderRow = this.MapToTableRow(tableRows.First()),
                DataRows = tableRows.Skip(1).Select(MapToTableRow).ToList()
            };
        }

        public TableRow MapToTableRowWithTestResult(G.TableRow tableRow)
        {
            if (tableRow == null)
            {
                return null;
            }

            return new TableRowWithTestResult(tableRow.Cells.Select(this.MapToString));
        }

        public Table MapToExampleTable(G.DataTable dataTable)
        {
            if (dataTable == null)
            {
                return null;
            }

            var tableRows = dataTable.Rows;
            return this.MapToExampleTable(tableRows);
        }

        public ExampleTable MapToExampleTable(IEnumerable<G.TableRow> tableRows)
        {
            return new ExampleTable
            {
                HeaderRow = this.MapToTableRow(tableRows.First()),
                DataRows = tableRows.Skip(1).Select(MapToTableRowWithTestResult).ToList()
            };
        }

        public string MapToString(G.DocString docString)
        {
            return docString?.Content;
        }

        public Step MapToStep(G.Step step)
        {
            if (step == null)
            {
                return null;
            }

            return new Step
            {
                Location = this.MapToLocation(step.Location),
                DocStringArgument = step.Argument is G.DocString ? this.MapToString((G.DocString) step.Argument) : null,
                Keyword = this.MapToKeyword(step.Keyword),
                NativeKeyword = step.Keyword,
                Name = step.Text,
                TableArgument = step.Argument is G.DataTable ? this.MapToTable((G.DataTable) step.Argument) : null,
            };
        }

        public Keyword MapToKeyword(string keyword)
        {
            if (keyword == null)
            {
                return default(Keyword);
            }

            keyword = keyword.Trim();

            if (this.languageServices.WhenStepKeywords.Contains(keyword))
            {
                return Keyword.When;
            }

            if (this.languageServices.GivenStepKeywords.Contains(keyword))
            {
                return Keyword.Given;
            }

            if (this.languageServices.ThenStepKeywords.Contains(keyword))
            {
                return Keyword.Then;
            }

            if (this.languageServices.AndStepKeywords.Contains(keyword))
            {
                return Keyword.And;
            }

            if (this.languageServices.ButStepKeywords.Contains(keyword))
            {
                return Keyword.But;
            }

            throw new ArgumentOutOfRangeException("keyword");
        }

        public string MapToString(G.Tag tag)
        {
            return tag?.Name;
        }

        public Comment MapToComment(G.Comment comment)
        {
            if (comment == null)
            {
                return null;
            }

            return new Comment
            {
                Text = comment.Text.Trim(),
                Location = this.MapToLocation(comment.Location)
            };
        }

        public Location MapToLocation(G.Location location)
        {
            return location != null ? new Location { Column = location.Column, Line = location.Line } : null;
        }

        public Scenario MapToScenario(G.Scenario scenario, params string[] tagsToHide)
        {
            if (scenario == null)
            {
                return null;
            }

            var visibleTags = RetrieveOnlyVisibleTags(scenario.Tags, tagsToHide);

            return new Scenario
            {
                Description = scenario.Description ?? string.Empty,
                Location = this.MapToLocation(scenario.Location),
                Name = scenario.Name,
                Slug = scenario.Name.ToSlug(),
                Steps = scenario.Steps.Select(this.MapToStep).ToList(),
                Tags = visibleTags
            };
        }

        public Example MapToExample(G.Examples examples)
        {
            if (examples == null)
            {
                return null;
            }

            return new Example
            {
                Description = examples.Description,
                Name = examples.Name,
                TableArgument = this.MapToExampleTable(((G.IHasRows) examples).Rows),
                Tags = examples.Tags?.Select(this.MapToString).ToList()
            };
        }

        public ScenarioOutline MapToScenarioOutline(G.ScenarioOutline scenarioOutline, params string[] tagsToHide)
        {
            if (scenarioOutline == null)
            {
                return null;
            }

            var visibleTags = RetrieveOnlyVisibleTags(scenarioOutline.Tags, tagsToHide);

            return new ScenarioOutline
            {
                Description = scenarioOutline.Description ?? string.Empty,
                Examples = (scenarioOutline.Examples ?? new G.Examples[0]).Select(this.MapToExample).ToList(),
                Location = this.MapToLocation(scenarioOutline.Location),
                Name = scenarioOutline.Name,
                Slug = scenarioOutline.Name.ToSlug(),
                Steps = scenarioOutline.Steps.Select(this.MapToStep).ToList(),
                Tags = visibleTags
            };
        }

        public Scenario MapToScenario(G.Background background)
        {
            if (background == null)
            {
                return null;
            }

            return new Scenario
            {
                Description = background.Description ?? string.Empty,
                Location = this.MapToLocation(background.Location),
                Name = background.Name,
                Steps = background.Steps.Select(this.MapToStep).ToList(),
            };
        }

        public Feature MapToFeature(G.GherkinDocument gherkinDocument)
        {
            if (gherkinDocument == null)
            {
                return null;
            }

            var tagsToHide = string.IsNullOrEmpty(configuration.HideTags) ? null : configuration.HideTags.Split(';');

            var feature = new Feature();

            var background = gherkinDocument.Feature.Children.SingleOrDefault(c => c is G.Background) as G.Background;
            if (background != null)
            {
                feature.AddBackground(this.MapToScenario(background));
            }

            //esr 02/26/2019 add theme and story tag parsing from feature comments - 
            if (this.configuration.ShouldEnableComments || this.configuration.commentParsing != "")
            {
                feature.Comments.AddRange((gherkinDocument.Comments ?? new G.Comment[0]).Select(this.MapToComment));
            }

            feature.Description = gherkinDocument.Feature.Description ?? string.Empty;

            foreach (var featureElement in gherkinDocument.Feature.Children.Where(c => !(c is G.Background)))
            {
                feature.AddFeatureElement(this.MapToFeatureElement(featureElement, tagsToHide));
            }

            feature.Name = gherkinDocument.Feature.Name;


            List<string> tags = RetrieveOnlyVisibleTags(gherkinDocument.Feature.Tags, tagsToHide);
            feature.Tags.AddRange(tags);

            foreach (var comment in feature.Comments.ToArray())
            {
                // Find the related feature
                var relatedFeatureElement =
                    feature.FeatureElements.LastOrDefault(x => x.Location.Line < comment.Location.Line);
                // Find the step to which the comment is related to
                if (relatedFeatureElement != null)
                {
                    var stepAfterComment =
                        relatedFeatureElement.Steps.FirstOrDefault(x => x.Location.Line > comment.Location.Line);
                    if (stepAfterComment != null)
                    {
                        // Comment is before a step
                        comment.Type = CommentType.StepComment;
                        stepAfterComment.Comments.Add(comment);
                    }
                    else
                    {
                        // Comment is located after the last step
                        var stepBeforeComment =
                            relatedFeatureElement.Steps.LastOrDefault(x => x.Location.Line < comment.Location.Line);
                        if (stepBeforeComment != null && stepBeforeComment == relatedFeatureElement.Steps.Last())
                        {

                            comment.Type = CommentType.AfterLastStepComment;
                            stepBeforeComment.Comments.Add(comment);
                        }
                    }
                }
            }

            //esr 02/26/2019 add theme and story tag parsing from feature comments - 
            if (this.configuration.commentParsing == "RCIS.CIMax")
            {
                IFeatureElement previousFeature = null;
                foreach (var featureElement in feature.FeatureElements.ToArray())
                {
                    featureElement.Feature = feature;
                    this.parseRCISComments(previousFeature, featureElement, feature.Comments);
                    previousFeature = featureElement;
                }
            }

            if (this.configuration.commentParsing != "" && !this.configuration.ShouldEnableComments)
            {
                foreach (var featureElement in feature.FeatureElements.ToArray())
                {
                    foreach (var step in featureElement.Steps)
                    {
                        step.Comments.Clear();
                    }
                }
                feature.Comments.Clear();
            }

            if (feature.Background != null)
            {
                feature.Background.Feature = feature;
            }

            feature.Language = gherkinDocument.Feature.Language ?? "en";

            return feature;
        }

        /// <summary>
        /// Parse feature comment for theme and story id's and add them as tags
        /// </summary>
        /// <param name="previousScenario"></param>
        /// <param name="currentScenario"></param>
        /// <param name="featureFileComments"></param>
        /// <param name="keepComments"></param>
        /// <comments>esr 02/26/2019 add theme and story tag parsing from feature comments - </comments>
        private void parseRCISComments(IFeatureElement previousScenario, IFeatureElement currentScenario,
            List<Comment> featureFileComments)
        {
            // apply parsed comments before first scenario in feature file to scenario
            if (previousScenario == null && featureFileComments.Count > 0)
            {
                bool gottostep = false;
                foreach (var cmt in featureFileComments)
                {
                    if (!gottostep)
                    {
                        if (cmt.Type == CommentType.Normal)
                        {
                            this.AddParsedTags(currentScenario, cmt);
                        }
                        else
                        {
                            gottostep = true;
                        }
                    }
                }
            }
            // apply parsed comments after last step of previous scenario to this scenario.
            if (previousScenario != null && previousScenario.Steps.Count>0)
            {
                foreach (var comment in previousScenario.Steps[previousScenario.Steps.Count-1].Comments)
                {
                    if (comment.Type == CommentType.AfterLastStepComment)
                    {
                        this.AddParsedTags(currentScenario, comment);
                    }
                }
            }
            // apply parsed comments from steps of this scenario to this scenario
            foreach (var step in currentScenario.Steps)
            {
                foreach (var comment in step.Comments)
                {
                    if (comment.Type != CommentType.AfterLastStepComment)
                    {
                        this.AddParsedTags(currentScenario, comment);
                    }
                }
            }
        }

        /// <summary>
        /// Parse story and theme id's from individual comment
        /// </summary>
        /// <param name="featureElement"></param>
        /// <param name="comment"></param>
        /// <comments>esr 02/25/2019 add theme and story tag parsing from feature comments -
        /// esr 02/26/2019 check for story tag with and without underscore</comments>
        private void AddParsedTags(IFeatureElement featureElement, Comment comment)
        {
            string tag = this.parseRCISTheme(comment);
            if (!string.IsNullOrEmpty(tag) && !featureElement.Tags.Contains(tag))
            {
                featureElement.Tags.Add(tag);
            }

            tag = this.parseRCISStory(comment);
            if (!string.IsNullOrEmpty(tag) &&
                !(featureElement.Tags.Contains($"@B_{tag}") || featureElement.Tags.Contains($"@B{tag}")))
            {
                featureElement.Tags.Add($"@B_{tag}");
            }
        }

        private static Regex _storyRegex = new Regex(@"\(B[-_](\d+)\)");
        /// <summary>
        /// Parse story id from feature file comment
        /// </summary>
        /// <param name="cmt"></param>
        /// <returns>story number</returns>
        /// <comments>esr 02/26/2019 add theme and story tag parsing from feature comments -
        /// esr 2/26/2019 don't add a story tag if story tag exists in old format without underscore</comments>
        private string parseRCISStory(Comment cmt)
        {
            string retval = null;
            Match match = _storyRegex.Match(cmt.Text);
            if (match.Success)
            {
                retval = match.Groups[1].Value;
            }

            return retval;
        }

        // esr 02/26/2019 don't parse RI:(B-12345) as theme
        private static Regex _themeRegex = new Regex(@"RI:\W*([^B(][A-Za-z0-9-]*)");
        /// <summary>
        /// Parse theme id from feature file comment
        /// </summary>
        /// <param name="cmt"></param>
        /// <returns>theme tag</returns>
        /// <comments>esr 02/25/2019 add theme and story tag parsing from feature comments -
        /// esr 02/26/2019 translate hyphen to underscore in theme</comments>
        private string parseRCISTheme(Comment cmt)
        {
            string retval = null;
            Match match = _themeRegex.Match(cmt.Text);
            if (match.Success)
            {
                string dest = "";
                foreach (var c in match.Groups[1].Value)
                {
                    if (c != "-"[0])
                    {
                        dest += c;
                    }
                    else
                    {
                        dest += "_";
                    }
                }
                retval =  $"@T_{dest}";
            }

            return retval;
        }

        private IFeatureElement MapToFeatureElement(G.ScenarioDefinition sd,params string[] tagsToHide)
        {
            if (sd == null)
            {
                return null;
            }

            var scenario = sd as G.Scenario;
            if (scenario != null)
            {
                return this.MapToScenario(scenario, tagsToHide);
            }

            var scenarioOutline = sd as G.ScenarioOutline;
            if (scenarioOutline != null)
            {
                return this.MapToScenarioOutline(scenarioOutline, tagsToHide);
            }

            var background = sd as G.Background;
            if (background != null)
            {
                return this.MapToScenario(background);
            }

            throw new ArgumentException("Only arguments of type Scenario, ScenarioOutline and Background are supported.");
        }

        private List<string> RetrieveOnlyVisibleTags(IEnumerable<G.Tag> originalTags,params string[] tagsToHide)
        {
            var usableTags = new List<string>();
            foreach (var tag in originalTags)
            {
                var stringTag = this.MapToString(tag);

                if (tagsToHide != null && tagsToHide.Any(t => stringTag.Equals($"@{t}", StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                usableTags.Add(stringTag);
            }
            return usableTags;
        }
    }
}
