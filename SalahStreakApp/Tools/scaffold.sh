#!/bin/bash

echo "🚀 Generating controllers for SalahStreak..."

# Array of models to generate controllers for
models=("Participant" "AgeGroup" "BiometricLog" "AttendanceCalendar" "AttendanceScore" "Round" "Winner" "Reward")

# Loop through each model
for model in "${models[@]}"; do
    echo "�� Generating ${model}Controller..."
    
    dotnet aspnet-codegenerator controller \
        -name "${model}Controller" \
        -m "$model" \
        -dc ApplicationDbContext \
        --relativeFolderPath Controllers
    
    if [ $? -eq 0 ]; then
        echo "✅ Generated ${model}Controller successfully"
    else
        echo "❌ Failed to generate ${model}Controller"
    fi
    
    echo "---"
done

echo "🎉 All controllers generated!"
echo "📁 Check the Controllers folder for your new controllers"
