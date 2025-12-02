using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomerSnapshot.Desktop;

public class MainForm : Form
{
    private readonly ICustomerSnapshotService _customerSnapshotService;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly TextBox _customerIdTextBox = new() { Width = 80 };
    private readonly Button _loadButton = new() { Text = "Load", AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true, ForeColor = System.Drawing.Color.DimGray };

    private readonly Label _customerNameValue = new() { AutoSize = true, Font = new("Segoe UI", 10, System.Drawing.FontStyle.Bold) };
    private readonly Label _customerSegmentValue = new() { AutoSize = true };
    private readonly Label _customerRefreshValue = new() { AutoSize = true };

    private readonly Label _kpiSummaryValue = new() { AutoSize = true, MaximumSize = new(420, 0) };
    private readonly TextBox _quotesValue = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _issuesValue = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _interactionsValue = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _talkingPointsValue = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _nextActionDetail = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Height = 60 };
    private readonly Label _nextActionLabel = new() { AutoSize = true, Font = new("Segoe UI", 10, System.Drawing.FontStyle.Bold) };

    public int? InitialCustomerId { get; set; }

    public MainForm(ICustomerSnapshotService customerSnapshotService)
    {
        _customerSnapshotService = customerSnapshotService;
        Text = "Customer Snapshot Viewer";
        MinimumSize = new(900, 700);
        StartPosition = FormStartPosition.CenterScreen;

        InitializeLayout();

        _loadButton.Click += async (_, _) => await LoadSnapshotAsync();
        Shown += async (_, _) =>
        {
            if (InitialCustomerId.HasValue)
            {
                _customerIdTextBox.Text = InitialCustomerId.Value.ToString();
                await LoadSnapshotAsync();
            }
        };
        FormClosing += (_, _) => _cancellationTokenSource.Cancel();
    }

    private async Task LoadSnapshotAsync()
    {
        if (!int.TryParse(_customerIdTextBox.Text, out var customerId))
        {
            _statusLabel.Text = "Enter a numeric customer ID.";
            return;
        }

        try
        {
            ToggleLoadingState(true);
            _statusLabel.Text = "Fetching snapshot...";

            var snapshot = await _customerSnapshotService.GetSnapshotAsync(customerId, _cancellationTokenSource.Token);
            if (snapshot is null)
            {
                _statusLabel.Text = "Customer not found.";
                ClearSnapshot();
                return;
            }

            RenderSnapshot(snapshot);
            _statusLabel.Text = $"Snapshot refreshed at {DateTime.Now:T}";
        }
        catch (OperationCanceledException)
        {
            _statusLabel.Text = "Request cancelled.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            ToggleLoadingState(false);
        }
    }

    private void ToggleLoadingState(bool isLoading)
    {
        _loadButton.Enabled = !isLoading;
        _customerIdTextBox.Enabled = !isLoading;
    }

    private void RenderSnapshot(CustomerSnapshotViewModel snapshot)
    {
        _customerNameValue.Text = $"{snapshot.Customer.Name} ({snapshot.Customer.Code})";
        _customerSegmentValue.Text = $"{snapshot.Customer.Segment} | {snapshot.Customer.Industry}";
        _customerRefreshValue.Text = snapshot.Customer.LastRefreshedUtc.ToLocalTime().ToString("g");

        _kpiSummaryValue.Text = FormatKpis(snapshot.Kpis);
        _quotesValue.Text = FormatQuotes(snapshot.OpenQuotes);
        _issuesValue.Text = FormatIssues(snapshot.OpenIssues);
        _interactionsValue.Text = FormatInteractions(snapshot.RecentInteractions);
        _talkingPointsValue.Text = FormatTalkingPoints(snapshot.TalkingPoints, snapshot.Personality);
        _nextActionLabel.Text = snapshot.NextAction.Label;
        _nextActionDetail.Text = snapshot.NextAction.Explanation;
    }

    private static string FormatKpis(CustomerKpis kpis)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"YTD Spend: {kpis.TotalSpendYtd:C}");
        builder.AppendLine($"Last Year: {kpis.TotalSpendLastYear:C} ({kpis.PercentDeltaVsLy:+0.0;-0.0;0.0}% vs LY)");
        builder.AppendLine($"Avg Order (YTD): {kpis.AverageOrderSizeYtd:C}");
        builder.AppendLine($"Avg Order (LY): {kpis.AverageOrderSizeLastYear:C}");
        builder.AppendLine($"Margin Trend: {kpis.MarginTrend}");
        builder.AppendLine($"Payment Behavior: {kpis.PaymentBehavior}, Avg Days to Pay: {kpis.AverageDaysToPay:0.0}");
        builder.AppendLine($"Invoices Late: {kpis.PercentInvoicesLate:0.0}%");
        builder.AppendLine($"Risk Rating: {kpis.RiskRating}");
        return builder.ToString();
    }

    private static string FormatQuotes(QuoteSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Open Quotes: {summary.OpenCount}");
        builder.AppendLine($"Total Value: {summary.TotalOpenValue:C}");
        builder.AppendLine();

        foreach (var quote in summary.TopQuotes)
        {
            builder.AppendLine($"#{quote.QuoteNumber} - {quote.Amount:C} ({quote.QuoteDate:d})");
            builder.AppendLine($"    {quote.Description}");
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatIssues(IssueSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Open Issues: {summary.OpenCount} {(summary.HasHighSeverity ? "(HIGH SEVERITY)" : string.Empty)}");
        builder.AppendLine();

        foreach (var issue in summary.TopIssues)
        {
            builder.AppendLine($"{issue.CreatedOn:d} - {issue.Severity} - {issue.Status}");
            builder.AppendLine($"    {issue.Summary}");
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatInteractions(IReadOnlyCollection<Interaction> interactions)
    {
        if (!interactions.Any())
        {
            return "No recent interactions recorded.";
        }

        var builder = new StringBuilder();
        foreach (var interaction in interactions)
        {
            builder.AppendLine($"{interaction.InteractionDate:g} - {interaction.InteractionType} ({interaction.Owner})");
            builder.AppendLine($"    {interaction.Subject}");
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatTalkingPoints(IReadOnlyCollection<TalkingPoint> talkingPoints, PersonalityProfile profile)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Communication: {profile.CommunicationPreference}, Value: {profile.ValueOrientation}, Response: {profile.ResponseCadence}");
        if (!string.IsNullOrWhiteSpace(profile.Notes))
        {
            builder.AppendLine($"Notes: {profile.Notes}");
        }
        builder.AppendLine();

        if (!talkingPoints.Any())
        {
            builder.AppendLine("No suggested talking points.");
        }
        else
        {
            var index = 1;
            foreach (var point in talkingPoints)
            {
                builder.AppendLine($"{index}. {point.Title}");
                builder.AppendLine($"   {point.Detail}");
                builder.AppendLine();
                index++;
            }
        }

        return builder.ToString().TrimEnd();
    }

    private void ClearSnapshot()
    {
        _customerNameValue.Text = string.Empty;
        _customerSegmentValue.Text = string.Empty;
        _customerRefreshValue.Text = string.Empty;
        _kpiSummaryValue.Text = string.Empty;
        _quotesValue.Text = string.Empty;
        _issuesValue.Text = string.Empty;
        _interactionsValue.Text = string.Empty;
        _talkingPointsValue.Text = string.Empty;
        _nextActionLabel.Text = string.Empty;
        _nextActionDetail.Text = string.Empty;
    }

    private void InitializeLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 50),
                new ColumnStyle(SizeType.Percent, 50)
            },
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100)
            },
            Padding = new Padding(10)
        };

        var headerLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        headerLayout.Controls.Add(new Label { Text = "Customer ID:", AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 4, 0) });
        headerLayout.Controls.Add(_customerIdTextBox);
        headerLayout.Controls.Add(_loadButton);
        headerLayout.Controls.Add(_statusLabel);

        var customerGroup = CreateCustomerGroup();
        var kpiGroup = CreateKpiGroup();
        var actionGroup = CreateActionGroup();

        var quotesGroup = CreateTextGroup("Open Quotes", _quotesValue);
        var issuesGroup = CreateTextGroup("Open Issues", _issuesValue);
        var interactionsGroup = CreateTextGroup("Recent Interactions", _interactionsValue);
        var talkingPointsGroup = CreateTextGroup("Talking Points & Personality", _talkingPointsValue);

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100)
            }
        };
        leftPanel.Controls.Add(customerGroup, 0, 0);
        leftPanel.Controls.Add(kpiGroup, 0, 1);
        leftPanel.Controls.Add(actionGroup, 0, 2);

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.Percent, 33),
                new RowStyle(SizeType.Percent, 33),
                new RowStyle(SizeType.Percent, 34)
            }
        };
        rightPanel.Controls.Add(quotesGroup, 0, 0);
        rightPanel.Controls.Add(issuesGroup, 0, 1);
        rightPanel.Controls.Add(interactionsGroup, 0, 2);

        var bottomPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 2,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 50),
                new ColumnStyle(SizeType.Percent, 50)
            }
        };
        bottomPanel.Controls.Add(talkingPointsGroup, 0, 0);
        bottomPanel.Controls.Add(CreateTextGroup("Next Best Action Details", _nextActionDetail), 1, 0);

        mainLayout.Controls.Add(headerLayout, 0, 0);
        mainLayout.SetColumnSpan(headerLayout, 2);
        mainLayout.Controls.Add(leftPanel, 0, 1);
        mainLayout.Controls.Add(rightPanel, 1, 1);
        mainLayout.SetRowSpan(leftPanel, 1);
        mainLayout.SetRowSpan(rightPanel, 1);
        mainLayout.Controls.Add(bottomPanel, 0, 2);
        mainLayout.SetColumnSpan(bottomPanel, 2);

        Controls.Add(mainLayout);
    }

    private GroupBox CreateCustomerGroup()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            ColumnStyles =
            {
                new ColumnStyle(SizeType.AutoSize),
                new ColumnStyle(SizeType.Percent, 100)
            }
        };

        layout.Controls.Add(new Label { Text = "Name", AutoSize = true }, 0, 0);
        layout.Controls.Add(_customerNameValue, 1, 0);
        layout.Controls.Add(new Label { Text = "Segment / Industry", AutoSize = true }, 0, 1);
        layout.Controls.Add(_customerSegmentValue, 1, 1);
        layout.Controls.Add(new Label { Text = "Last Refreshed", AutoSize = true }, 0, 2);
        layout.Controls.Add(_customerRefreshValue, 1, 2);

        return new GroupBox
        {
            Text = "Customer",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Controls = { layout }
        };
    }

    private GroupBox CreateKpiGroup()
    {
        return new GroupBox
        {
            Text = "KPIs",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Controls = { _kpiSummaryValue },
            Padding = new Padding(10)
        };
    }

    private GroupBox CreateActionGroup()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100)
            }
        };

        layout.Controls.Add(_nextActionLabel, 0, 0);
        layout.Controls.Add(_nextActionDetail, 0, 1);

        return new GroupBox
        {
            Text = "Next Best Action",
            Dock = DockStyle.Fill,
            Controls = { layout }
        };
    }

    private static GroupBox CreateTextGroup(string title, TextBox textBox)
    {
        return new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Controls = { textBox }
        };
    }
}
