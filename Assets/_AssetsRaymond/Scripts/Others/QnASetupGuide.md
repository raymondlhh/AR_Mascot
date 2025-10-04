# QnA Manager Setup Guide

## Overview
The QnaManager now includes all the text typing and audio features from ChatGPT, but uses answers from your text file instead of the ChatGPT API.

## Features Included
- ✅ Text typing animation (character by character)
- ✅ Audio voiceover during typing
- ✅ 3D text display support
- ✅ Regular text display with fade-out
- ✅ Mascot dialogue integration
- ✅ Button and input field management
- ✅ Answer variations and randomization
- ✅ Complete text file parsing

## Setup Steps

### 1. Copy Text File to Resources
1. Add the `TextFileSetup` script to any GameObject in your scene
2. In the Inspector, right-click on the script and select "Copy Text File to Resources"
3. This will copy your `AR Mascot.txt` file to `Assets/Resources/AR_Mascot.txt`

### 2. Assign Text File to QnaManager
1. Select the QnaManager GameObject in your scene
2. In the Inspector, drag the `AR_Mascot.txt` file to the "Qna Text File" field
3. The system will automatically parse the text file on Start()

### 3. Copy References from ChatGPT
1. With the TextFileSetup script selected, right-click and choose "Setup QnaManager References"
2. This will automatically copy all UI and audio references from ChatGPT to QnaManager
3. Verify that all fields are now populated in the QnaManager Inspector

### 4. Configure QnaManager Settings
In the QnaManager Inspector, you can adjust:
- **Enable Variations**: Check this to get randomized answers
- **Interval**: Time between text chunks (default: 1 second)
- **Is 3D Text**: Whether to use 3D text display or regular UI text

### 5. Test the System
1. Play the scene
2. Select a question from the dropdown
3. Click the Send button
4. The system should display the answer with typing animation and audio

## How It Works

### Text File Parsing
The system automatically parses your text file and extracts:
- Questions and their variations
- Team information (all teams and members)
- Course descriptions
- Special cases and responses
- Answer variations for randomization

### Answer Selection Process
1. **Exact Match**: Tries to find exact question matches first
2. **Partial Match**: Looks for partial matches in questions
3. **Keyword Matching**: Uses keywords to find related answers
4. **Default Response**: Returns a polite message for unrelated questions

### Display Features
- **3D Text Mode**: Uses MascotDialogue for 3D text display
- **Regular Text Mode**: Uses UI Text with typing animation
- **Audio Integration**: Plays typing sounds during text display
- **Fade Effects**: Text fades out after display

## Supported Questions

### About Rendify
- "Who are you?"
- "What is your name?"
- "What is your slogan?"
- "Who developed you?"
- "Who made the DMT Showcase?"

### About Courses
- "Tell me about the FYP"
- "What is VR/AR?"
- "Tell me about Video Games"
- "What is a Board Game?"
- "Tell me about 3D Modeling"
- "What is 2D/3D Animation?"
- "Tell me about Game Environments"
- "What is Video/Audio Production?"

### About Teams
- "Who is in the [Team Name] Team?"
- "Who are the Student Helpers?"
- "Who is the Head of DMT?"

### Special Questions
- "How many courses are in the showcase?"
- "Which course is the most creative?"
- "Which course is the most technical?"

## Troubleshooting

### Common Issues
1. **"QnA Text File is not assigned!"** - Make sure the text file is assigned in the QnaManager
2. **No audio playing** - Check that AudioManager is assigned and has "Typing" voiceover
3. **Text not displaying** - Verify that displayText or mascotDialogue is assigned
4. **Button not working** - Ensure the button reference is assigned

### Debug Information
- The system logs the number of loaded Q&A pairs to the console
- Check the Console for any error messages
- Use the "Validate Text File" option in TextFileSetup to check file parsing

## Benefits Over ChatGPT
- **No API Costs**: No external API calls required
- **Faster Response**: Instant local responses
- **Consistent Content**: Exact control over information provided
- **Offline Capable**: Works without internet connection
- **Customizable**: Easy to modify answers and add new questions
- **Same User Experience**: Identical typing animation and audio effects

## Customization

### Adding New Questions
1. Add new question-answer pairs to the text file
2. Follow the existing format
3. The system will automatically parse and include them

### Modifying Responses
1. Edit the text file directly
2. The system will reload the data on the next play
3. For runtime changes, modify the `AddSpecialCases()` method in QnaManager

### Adjusting Display Settings
- **Interval**: Change the time between text chunks
- **Typing Speed**: Modify the character delay in `DisplayTextInChunksRegular`
- **Fade Duration**: Adjust the fade-out time in `FadeOutText`
