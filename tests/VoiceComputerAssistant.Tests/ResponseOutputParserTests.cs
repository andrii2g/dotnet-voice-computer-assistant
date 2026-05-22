using System.Text.Json;
using VoiceComputerAssistant.App.OpenAI;

namespace VoiceComputerAssistant.Tests;

public sealed class ResponseOutputParserTests
{
    private readonly ResponseOutputParser _parser = new();

    [Fact]
    public void Finds_ComputerCall_With_Screenshot_Action()
    {
        var response = ParseResponse(
            """
            {
              "id": "resp_1",
              "output": [
                {
                  "type": "computer_call",
                  "call_id": "call_1",
                  "actions": [
                    { "type": "screenshot" }
                  ]
                }
              ]
            }
            """);

        var computerCall = _parser.FindComputerCall(response);

        Assert.NotNull(computerCall);
        Assert.Equal("call_1", computerCall!.CallId);
        Assert.Single(computerCall.Actions);
        Assert.Equal("screenshot", computerCall.Actions[0].Type);
    }

    [Fact]
    public void Finds_ComputerCall_With_Click_Type_And_Wait_Actions()
    {
        var response = ParseResponse(
            """
            {
              "id": "resp_2",
              "output": [
                {
                  "type": "computer_call",
                  "call_id": "call_2",
                  "actions": [
                    { "type": "click", "x": 120, "y": 80, "button": "left" },
                    { "type": "type", "text": "dotnet" },
                    { "type": "wait" }
                  ]
                }
              ]
            }
            """);

        var computerCall = _parser.FindComputerCall(response);

        Assert.NotNull(computerCall);
        Assert.Equal(3, computerCall!.Actions.Count);
        Assert.Equal("click", computerCall.Actions[0].Type);
        Assert.Equal(120, computerCall.Actions[0].X);
        Assert.Equal("type", computerCall.Actions[1].Type);
        Assert.Equal("dotnet", computerCall.Actions[1].Text);
        Assert.Equal("wait", computerCall.Actions[2].Type);
    }

    [Fact]
    public void Extracts_Root_Level_OutputText()
    {
        var response = ParseResponse(
            """
            {
              "id": "resp_3",
              "output_text": "Visible projects are filtered to dotnet.",
              "output": []
            }
            """);

        var text = _parser.ExtractFinalText(response);

        Assert.Equal("Visible projects are filtered to dotnet.", text);
    }

    [Fact]
    public void Extracts_Message_Content_Text_When_OutputText_Is_Absent()
    {
        var response = ParseResponse(
            """
            {
              "id": "resp_4",
              "output": [
                {
                  "type": "message",
                  "content": [
                    {
                      "type": "output_text",
                      "text": "The greenhouse project is a small IoT dashboard."
                    }
                  ]
                }
              ]
            }
            """);

        var text = _parser.ExtractFinalText(response);

        Assert.Equal("The greenhouse project is a small IoT dashboard.", text);
    }

    [Fact]
    public void Returns_Null_When_No_ComputerCall_Exists()
    {
        var response = ParseResponse(
            """
            {
              "id": "resp_5",
              "output": [
                {
                  "type": "message",
                  "content": [
                    { "type": "output_text", "text": "done" }
                  ]
                }
              ]
            }
            """);

        var computerCall = _parser.FindComputerCall(response);

        Assert.Null(computerCall);
    }

    private static ResponsesApiResponse ParseResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement.Clone();
        return new ResponsesApiResponse(root.GetProperty("id").GetString()!, json, root);
    }
}
