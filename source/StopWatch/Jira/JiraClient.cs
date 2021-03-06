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
using System;
using System.Linq;
using System.Collections.Generic;

namespace StopWatch
{
    internal class JiraClient
    {
        public bool SessionValid { get; private set; }

        public string ErrorMessage { get; private set; }

        #region public methods
        public JiraClient(IJiraApiRequestFactory jiraApiRequestFactory, IJiraApiRequester jiraApiRequester)
        {
            this.jiraApiRequestFactory = jiraApiRequestFactory;
            this.jiraApiRequester = jiraApiRequester;

            SessionValid = false;
            ErrorMessage = "";
        }


        public bool Authenticate(string username, string apiToken)
        {
            SessionValid = false;
            jiraApiRequester.SetAuthentication(username, apiToken);
            JiraTimeHelpers.Configuration = GetTimeTrackingConfiguration();
            return JiraTimeHelpers.Configuration != null;
        }


        public bool ValidateSession()
        {
            SessionValid = false;

            var request = jiraApiRequestFactory.CreateValidateSessionRequest();
            try
            {
                jiraApiRequester.DoAuthenticatedRequest<object>(request);
                SessionValid = true;
                return true;
            }
            catch (RequestDeniedException)
            {
                ErrorMessage = jiraApiRequester.ErrorMessage;
                return false;
            }
        }


        public List<Filter> GetFavoriteFilters()
        {
            var request = jiraApiRequestFactory.CreateGetFavoriteFiltersRequest();
            try
            {
                return jiraApiRequester.DoAuthenticatedRequest<List<Filter>>(request);
            }
            catch (RequestDeniedException)
            {
                return null;
            }
        }


        public SearchResult GetIssuesByJQL(string jql)
        {
            var request = jiraApiRequestFactory.CreateGetIssuesByJQLRequest(jql);
            try
            {
                return jiraApiRequester.DoAuthenticatedRequest<SearchResult>(request);
            }
            catch (RequestDeniedException)
            {
                return null;
            }
        }

        public Worklogs GetWorklogs(string key)
        {
            var request = jiraApiRequestFactory.CreateGetWorklogsRequest(key);
            return jiraApiRequester.DoAuthenticatedRequest<Worklogs>(request);
        }

        public Issue GetIssue(string key)
        {
            var request = jiraApiRequestFactory.CreateGetIssueRequest(key);
            return jiraApiRequester.DoAuthenticatedRequest<Issue>(request);
        }

        public TimetrackingFields GetIssueTimetracking(string key)
        {
            var request = jiraApiRequestFactory.CreateGetIssueTimetrackingRequest(key);
            try
            {
                return jiraApiRequester.DoAuthenticatedRequest<Issue>(request).Fields.Timetracking;
            }
            catch (RequestDeniedException)
            {
                return null;
            }
        }


        public TimeTrackingConfiguration GetTimeTrackingConfiguration()
        {
            var request = jiraApiRequestFactory.CreateGetConfigurationRequest();
            try
            {
                return jiraApiRequester.DoAuthenticatedRequest<JiraConfiguration>(request).timeTrackingConfiguration;
            }
            catch (RequestDeniedException)
            {
                return null;
            }
        }

        public bool PostWorklog(string key, DateTimeOffset startTime, TimeSpan time, string comment, EstimateUpdateMethods estimateUpdateMethod, string estimateUpdateValue)
        {
            var request = jiraApiRequestFactory.CreatePostWorklogRequest(key, startTime, time, comment, estimateUpdateMethod, estimateUpdateValue);
            try
            {
                jiraApiRequester.DoAuthenticatedRequest<object>(request);
                return true;
            }
            catch (RequestDeniedException)
            {
                return false;
            }
        }


        public bool PostComment(string key, string comment)
        {
            var request = jiraApiRequestFactory.CreatePostCommentRequest(key, comment);
            try
            {
                jiraApiRequester.DoAuthenticatedRequest<object>(request);
                return true;
            }
            catch (RequestDeniedException)
            {
                return false;
            }
        }


        public AvailableTransitions GetAvailableTransitions(string key)
        {
            var request = jiraApiRequestFactory.CreateGetAvailableTransitions(key);
            try
            {
                return jiraApiRequester.DoAuthenticatedRequest<AvailableTransitions>(request);
            }
            catch (RequestDeniedException)
            {
                return null;
            }
        }


        public bool DoTransition(string key, int transitionId)
        {
            var request = jiraApiRequestFactory.CreateDoTransition(key, transitionId);
            try
            {
                jiraApiRequester.DoAuthenticatedRequest<object>(request);
                return true;
            }
            catch (RequestDeniedException)
            {
                return false;
            }
        }

        public void ChangeIssueState(string key, string[] transitionList)
        {
            if (transitionList.Length <= 0)
                return;

            transitionList = transitionList.Select(s => s.ToLowerInvariant()).ToArray();

            var availableTransitions = GetAvailableTransitions(key);
            if (availableTransitions == null || availableTransitions.Transitions.Count == 0)
                return;

            foreach (var t in availableTransitions.Transitions)
            {
                if (transitionList.Any(t.Name.ToLower().Contains))
                {
                    DoTransition(key, t.Id);
                    return;
                }
            }
        }
        #endregion

        #region private members
        private IJiraApiRequestFactory jiraApiRequestFactory;
        private IJiraApiRequester jiraApiRequester;
        #endregion
    }
}
