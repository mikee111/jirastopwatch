﻿/**************************************************************************
Copyright 2016 Carsten Gehling

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**************************************************************************/
namespace StopWatch
{
    internal class IssueFields
    {
        public string Summary { get; set; }
        public TimetrackingFields Timetracking { get; set; }
        public ProjectFields Project { get; set; }
        public IssueType IssueType { get; set; }

        [RestSharp.Deserializers.DeserializeAs(Name = "customfield_10014")]
        public float StoryPoints { get; set; }
    }

    internal class TimetrackingFields
    {
        public string RemainingEstimate { get; set; }
        public int RemainingEstimateSeconds { get; set; }
    }

    internal class ProjectFields
    {
        public string Name { get; set; }
    }

    internal class IssueType
    {
        public string Name { get; set; }
    }
}
