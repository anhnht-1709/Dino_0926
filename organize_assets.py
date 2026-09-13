import os
import shutil
import glob

# Create assets folder
if not os.path.exists("assets"):
    os.makedirs("assets")

# Find all loose assets
files_to_move = []
for ext in ["*.png", "*.wav", "*.ttf"]:
    files_to_move.extend(glob.glob(ext))

moved_files = []

for file in files_to_move:
    if not os.path.isfile(file):
        continue
    # Move the file
    target = os.path.join("assets", file)
    shutil.move(file, target)
    moved_files.append(file)
    print(f"Moved {file}")
    
    # Move the .import file if it exists
    import_file = file + ".import"
    if os.path.exists(import_file):
        shutil.move(import_file, os.path.join("assets", import_file))

# Update references in .cs and .tscn
text_files = glob.glob("*.cs") + glob.glob("*.tscn")

for txt_file in text_files:
    with open(txt_file, "r", encoding="utf-8") as f:
        content = f.read()
        
    original_content = content
    for moved in moved_files:
        # Avoid replacing things like res://Clothes/filename if they somehow matched (though moved_files are loose)
        # We replace exactly "res://filename" with "res://assets/filename"
        search_str = f"res://{moved}"
        replace_str = f"res://assets/{moved}"
        content = content.replace(search_str, replace_str)
        
    if content != original_content:
        with open(txt_file, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"Updated references in {txt_file}")

print("Done organizing assets!")
