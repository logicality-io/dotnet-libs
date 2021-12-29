﻿namespace Logicality.GitHubActionsWorkflowBuilder;

public class WorkflowBuilder
{
    private readonly string                          _name;
    private readonly List<TriggerBuilder>           _triggers = new();
    private readonly Dictionary<string, JobBuilder> _jobs     = new();

    public WorkflowBuilder(string name)
    {
        _name = name;
    }

    public IVcsTriggerBuilder OnPullRequest() => On("pull_request");

    public IVcsTriggerBuilder OnPush() => On("push");

    public IVcsTriggerBuilder On(string eventName)
    {
        var trigger = new VcsTriggerBuilder(eventName, this);
        _triggers.Add(trigger);
        return trigger;
    }

    public IJobBuilder AddJob(string jobId)
    {
        var job = new JobBuilder(jobId);
        _jobs.Add(jobId, job);
        return job;
    }

    public string Generate()
    {
        var writer = new WorkflowWriter();
        writer.WriteLine("# This was generated by a tool (Logicality GitHub Actions Workflow Builder).");
        writer.WriteLine("# Edits will be overwritten.").WriteLine("");
        writer.WriteLine($"name: {_name}").WriteLine("");

        if (_triggers.Any())
        {
            writer.WriteLine("on:");
        }

        foreach (var trigger in _triggers)
        {
            using (writer.Indent())
            {
                trigger.Write(writer);
            }
        }

        if (_jobs.Any())
        {
            writer.WriteLine("");
            writer.WriteLine("jobs:");
            foreach (var job in _jobs)
            {
                using var _ = writer.Indent();
                writer.WriteLine($"{job.Key}:");
                using var __ = writer.Indent();
                job.Value.Write(writer);
            }
        }

        return writer.ToString();
    }

    private abstract class TriggerBuilder : ITriggerBuilder
    {
        protected TriggerBuilder(string eventName, WorkflowBuilder workflowBuilder)
        {
            EventName  = eventName;
            WorkflowBuilder = workflowBuilder;
        }

        public string          EventName       { get; }
        public WorkflowBuilder WorkflowBuilder { get; }

        public abstract void Write(WorkflowWriter writer);
    }

    private class VcsTriggerBuilder : TriggerBuilder, IVcsTriggerBuilder
    {
        private string[] _branches       = Array.Empty<string>();
        private string[] _branchesIgnore = Array.Empty<string>();
        private string[] _paths          = Array.Empty<string>();
        private string[] _pathsIgnore    = Array.Empty<string>();
        private string[] _tags           = Array.Empty<string>();
        private string[] _tagsIgnore     = Array.Empty<string>();

        public VcsTriggerBuilder(string eventName, WorkflowBuilder workflowBuilder)
            : base(eventName, workflowBuilder)
        {
        }

        public IVcsTriggerBuilder Branches(params string[] branches)
        {
            _branches = branches;
            return this;
        }

        public IVcsTriggerBuilder BranchesIgnore(params string[] branches)
        {
            _branchesIgnore = branches;
            return this;
        }

        public IVcsTriggerBuilder Paths(params string[] paths)
        {
            _paths = paths;
            return this;
        }

        public IVcsTriggerBuilder PathsIgnore(params string[] paths)
        {
            _pathsIgnore = paths;
            return this;
        }

        public IVcsTriggerBuilder Tags(params string[] tags)
        {
            _tags = tags;
            return this;
        }

        public IVcsTriggerBuilder TagsIgnore(params string[] tags)
        {
            _tagsIgnore = tags;
            return this;
        }

        public override void Write(WorkflowWriter writer)
        {
            writer.WriteLine($"{EventName}:");
            using var _ = writer.Indent();

            if (_branches.Any())
            {
                writer.WriteLine("branches:");
                foreach (var branch in _branches)
                {
                    writer.WriteLine($"- {branch}");
                }
            }
            if (_branchesIgnore.Any())
            {
                writer.WriteLine("branches-ignore:");
                foreach (var branch in _branchesIgnore)
                {
                    writer.WriteLine($"- {branch}");
                }
            }

            if (_paths.Any())
            {
                writer.WriteLine("paths:");
                foreach (var path in _paths)
                {
                    writer.WriteLine($"- {path}");
                }
            }
            if (_pathsIgnore.Any())
            {
                writer.WriteLine("paths-ignore:");
                foreach (var path in _pathsIgnore)
                {
                    writer.WriteLine($"- {path}");
                }
            }

            if (_tags.Any())
            {
                writer.WriteLine("tags:");
                foreach (var tags in _tags)
                {
                    writer.WriteLine($"- {tags}");
                }
            }
            if (_tagsIgnore.Any())
            {
                writer.WriteLine("tags-ignore:");
                foreach (var tags in _tagsIgnore)
                {
                    writer.WriteLine($"- {tags}");
                }
            }
        }
    }

    private class JobBuilder : IJobBuilder
    {
        private readonly string                      _jobId;
        private          string                      _runsOn;
        private          IDictionary<string, string> _environment = new Dictionary<string, string>();
        private readonly List<StepBuilder>           _steps       = new();

        public JobBuilder(string jobId)
        {
            _jobId = jobId;
        }

        public IJobBuilder RunsOn(string runsOn)
        {
            _runsOn = runsOn;
            return this;
        }

        public IJobBuilder WithEnvironment(IDictionary<string, string> environment)
        {
            _environment = environment;
            return this;
        }

        public IStepBuilder AddStep()
        {
            var step = new StepBuilder(this);
            _steps.Add(step);
            return step;
        }

        public void Write(WorkflowWriter writer)
        {
            writer.WriteLine($"runs-on: {_runsOn}");
            if (_environment.Any())
            {
                writer.WriteLine("env:");
                using (writer.Indent())
                {
                    foreach (var env in _environment)
                    {
                        writer.WriteLine($"{env.Key}: {env.Value}");
                    }
                }
            }

            if (_steps.Any())
            {
                writer.WriteLine("");
                writer.WriteLine("steps:");
                foreach (var step in _steps)
                {
                    writer.WriteLine("");
                    step.Write(writer);
                }
            }
        }
    }

    private class StepBuilder : IStepBuilder
    {
        private          string?                _name;
        private          string?                _conditional;
        private          string?                _uses;
        private readonly List<(string, string)> _with = new();
        private          string?                _run;
        private          string?                _shell;

        public StepBuilder(IJobBuilder job)
        {
            Job = job;
        }

        public IJobBuilder Job { get; }

        public IStepBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public IStepBuilder If(string conditional)
        {
            _conditional = conditional;
            return this;
        }

        public IStepBuilder Uses(string uses)
        {
            _uses = uses;
            return this;
        }

        public IStepBuilder With(string name, string value)
        { 
            _with.Add((name, value));
            return this;
        }

        public IStepBuilder Run(string run)
        {
            _run = run;
            return this;
        }

        public IStepBuilder Shell(string shell)
        {
            _shell = shell;
            return this;
        }

        public void Write(WorkflowWriter writer)
        {
            writer.WriteLine($"- name: {_name}");
            if (!string.IsNullOrWhiteSpace(_conditional))
            {
                using var _ = writer.Indent();
                writer.WriteLine($"if: {_conditional}");
            }

            if (!string.IsNullOrWhiteSpace(_uses))
            {
                using var _ = writer.Indent();
                writer.WriteLine($"uses: {_uses}");

                if (_with.Any())
                {
                    writer.WriteLine("with:");
                    using var __ = writer.Indent();
                    foreach (var (item1, item2) in _with)
                    {
                        writer.WriteLine($"{item1}: {item2}");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_run))
            {
                using var __ = writer.Indent();
                writer.WriteLine($"run: {_run}");
            }

            if (!string.IsNullOrWhiteSpace(_shell))
            {
                using var __ = writer.Indent();
                writer.WriteLine($"shell: {_shell}");
            }
        }
    }
}

public interface IJobBuilder
{
    IJobBuilder RunsOn(string runsOn);

    IJobBuilder WithEnvironment(IDictionary<string, string> environment);

    IStepBuilder AddStep();
}

public interface IStepBuilder
{
    IJobBuilder Job { get; }

    IStepBuilder Name(string name);

    IStepBuilder If(string condition);

    IStepBuilder Uses(string uses);

    IStepBuilder With(string name, string value);

    IStepBuilder Run(string run);

    IStepBuilder Shell(string run);
}
