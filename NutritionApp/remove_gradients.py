import os
import re
import glob

target_dir = r"c:\Users\stepa\diplomik\NutritionApp\NutritionApp\Views"
xaml_files = glob.glob(os.path.join(target_dir, "*.xaml"))

regex = re.compile(r'\s*<Grid\.Background>\s*<LinearGradientBrush StartPoint="0,0" EndPoint="1,1">.*?</Grid\.Background>', re.DOTALL)

for file_path in xaml_files:
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()
    
    new_content, num_subs = regex.subn('', content)
    
    if num_subs > 0:
        print(f"Removed {num_subs} gradient(s) from {os.path.basename(file_path)}")
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(new_content)
