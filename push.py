import os
import shutil
import subprocess
import sys
from datetime import datetime

# Ensure utf-8 encoding for console output on Windows
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass

# ============================================================
# CONFIGURATION
# ============================================================

# Main working project
HOTEL_DIR = r"C:\HotelManagement"

# HotelManagement copy inside CSE repository
CSE_HOTEL_DIR = r"C:\CSE\Third Year\3-2\Software Engineering\HotelManagement"

# CSE repository root
CSE_DIR = r"C:\CSE"


# ============================================================
# GIT HELPER
# ============================================================


def run_git(command, repo_dir, env=None):
    """
    Run a Git command inside the specified repository.
    Returns True if successful.
    """

    try:
        subprocess.run(command, cwd=repo_dir, env=env, check=True)

        return True

    except subprocess.CalledProcessError:
        return False


def git_has_changes(repo_dir):
    """
    Check whether the repository contains any changes.
    """

    result = subprocess.run(
        ["git", "status", "--porcelain"], cwd=repo_dir, capture_output=True, text=True
    )

    return bool(result.stdout.strip())


# ============================================================
# STEP 1
# EFFICIENTLY SYNC HOTELMANAGEMENT → CSE
# ============================================================


def sync_hotel_to_cse():
    print()
    print("==============================================")
    print("   Syncing HotelManagement → CSE")
    print("==============================================")
    print()

    if not os.path.exists(HOTEL_DIR):
        print(f"❌ HotelManagement folder not found:\n{HOTEL_DIR}")

        return False

    os.makedirs(CSE_HOTEL_DIR, exist_ok=True)

    # --------------------------------------------------------
    # Files/folders that should NOT be synchronized
    # --------------------------------------------------------

    ignored_names = {".git", "bin", "obj", "push.py"}

    copied_count = 0
    skipped_count = 0
    deleted_count = 0

    # ========================================================
    # PART A
    # COPY NEW / MODIFIED FILES ONLY
    # ========================================================

    for root, dirs, files in os.walk(HOTEL_DIR):
        # ----------------------------------------------------
        # Remove ignored directories from traversal
        # ----------------------------------------------------

        dirs[:] = [d for d in dirs if d not in ignored_names]

        # ----------------------------------------------------
        # Determine relative directory
        # ----------------------------------------------------

        relative_root = os.path.relpath(root, HOTEL_DIR)

        if relative_root == ".":
            destination_root = CSE_HOTEL_DIR

        else:
            destination_root = os.path.join(CSE_HOTEL_DIR, relative_root)

        os.makedirs(destination_root, exist_ok=True)

        # ----------------------------------------------------
        # Process files
        # ----------------------------------------------------

        for filename in files:
            # Never copy push.py
            if filename in ignored_names:
                skipped_count += 1
                continue

            source_file = os.path.join(root, filename)

            destination_file = os.path.join(destination_root, filename)

            # ------------------------------------------------
            # New file
            # ------------------------------------------------

            if not os.path.exists(destination_file):
                shutil.copy2(source_file, destination_file)

                copied_count += 1

                print(f"📄 Added: {os.path.relpath(source_file, HOTEL_DIR)}")

                continue

            # ------------------------------------------------
            # Existing file
            #
            # Compare size + modification time
            # ------------------------------------------------

            source_stat = os.stat(source_file)

            destination_stat = os.stat(destination_file)

            source_size = source_stat.st_size
            destination_size = destination_stat.st_size

            source_mtime = source_stat.st_mtime
            destination_mtime = destination_stat.st_mtime

            # ------------------------------------------------
            # Copy only if changed
            # ------------------------------------------------

            if (
                source_size != destination_size
                or abs(source_mtime - destination_mtime) > 1
            ):
                shutil.copy2(source_file, destination_file)

                copied_count += 1

                print(f"🔄 Updated: {os.path.relpath(source_file, HOTEL_DIR)}")

            else:
                skipped_count += 1

    # ========================================================
    # PART B
    # DELETE FILES THAT NO LONGER EXIST IN SOURCE
    # ========================================================

    for root, dirs, files in os.walk(CSE_HOTEL_DIR, topdown=False):
        # ----------------------------------------------------
        # Never touch .git
        # ----------------------------------------------------

        dirs[:] = [d for d in dirs if d != ".git"]

        relative_root = os.path.relpath(root, CSE_HOTEL_DIR)

        if relative_root == ".":
            source_root = HOTEL_DIR

        else:
            source_root = os.path.join(HOTEL_DIR, relative_root)

        # ----------------------------------------------------
        # Delete files missing from source
        # ----------------------------------------------------

        for filename in files:
            # push.py is intentionally not synchronized.
            # If it exists in CSE, leave it alone.
            if filename == "push.py":
                continue

            destination_file = os.path.join(root, filename)

            source_file = os.path.join(source_root, filename)

            if not os.path.exists(source_file):
                try:
                    os.remove(destination_file)

                    deleted_count += 1

                    print(
                        f"🗑️ Deleted: {os.path.relpath(destination_file, CSE_HOTEL_DIR)}"
                    )

                except OSError as e:
                    print(f"⚠️ Could not delete {destination_file}: {e}")

        # ----------------------------------------------------
        # Delete empty directories that no longer exist
        # ----------------------------------------------------

        for dirname in dirs:
            if dirname in ignored_names:
                continue

            destination_dir = os.path.join(root, dirname)

            source_dir = os.path.join(source_root, dirname)

            if not os.path.exists(source_dir):
                try:
                    shutil.rmtree(destination_dir)

                    deleted_count += 1

                    print(
                        f"🗑️ Deleted folder: "
                        f"{os.path.relpath(destination_dir, CSE_HOTEL_DIR)}"
                    )

                except OSError as e:
                    print(f"⚠️ Could not delete {destination_dir}: {e}")

    # ========================================================
    # SUMMARY
    # ========================================================

    print()
    print("----------------------------------------------")
    print("Sync Summary")
    print("----------------------------------------------")
    print(f"📄 Added/Updated : {copied_count}")
    print(f"⏭️ Unchanged     : {skipped_count}")
    print(f"🗑️ Deleted       : {deleted_count}")
    print("----------------------------------------------")
    print()

    print("✅ HotelManagement sync completed.")

    return True


# ============================================================
# STEP 2
# HOTELMANAGEMENT REPOSITORY
# NORMAL COMMIT + PUSH
# ============================================================


def push_hotel_management():
    print()
    print("==============================================")
    print("   HotelManagement GitHub")
    print("==============================================")
    print()

    if git_has_changes(HOTEL_DIR):
        print("📦 Staging HotelManagement changes...")

        if not run_git(["git", "add", "."], HOTEL_DIR):
            print("❌ Failed to stage HotelManagement.")

            return False

        print("📝 Creating HotelManagement commit...")

        result = subprocess.run(
            ["git", "commit", "-m", "Update HotelManagement"], cwd=HOTEL_DIR
        )

        if result.returncode != 0:
            print("❌ HotelManagement commit failed.")

            return False
    else:
        print("ℹ️ HotelManagement: No uncommitted changes.")

    print("🚀 Pushing HotelManagement to GitHub...")

    if run_git(["git", "push"], HOTEL_DIR):
        print("✅ HotelManagement pushed successfully.")

        return True

    print("❌ HotelManagement push failed.")

    return False


# ============================================================
# STEP 3
# GET CHANGED FILES FROM CSE HOTELMANAGEMENT
# ============================================================


def get_cse_hotel_changes():
    try:
        result = subprocess.run(
            ["git", "status", "--porcelain", "--untracked-files=all"],
            cwd=CSE_DIR,
            capture_output=True,
            text=True,
            check=True,
        )

    except subprocess.CalledProcessError:
        print("❌ Unable to read CSE Git status.")

        return []

    changes = []

    normalized_target = os.path.relpath(CSE_HOTEL_DIR, CSE_DIR).replace("/", "\\").rstrip("\\")

    for line in result.stdout.splitlines():
        if not line.strip():
            continue

        status = line[:2]
        filepath = line[3:].strip()

        # Handle git rename format: "R  old -> new" or "R  \"old\" -> \"new\""
        if " -> " in filepath:
            filepath = filepath.split(" -> ")[1].strip()

        # Git quotes paths containing spaces/special characters
        if filepath.startswith('"') and filepath.endswith('"'):
            filepath = filepath[1:-1]

        normalized_path = filepath.replace("/", "\\")

        if normalized_path.lower().startswith(normalized_target.lower() + "\\"):
            changes.append((status, filepath))

    return changes


# ============================================================
# STEP 4
# GET FILE MODIFICATION TIME
# ============================================================


def get_file_timestamp(filepath):
    absolute_path = os.path.join(CSE_DIR, filepath)

    if os.path.exists(absolute_path):
        return os.path.getmtime(absolute_path)

    # Deleted files no longer have a filesystem
    # modification timestamp.
    #
    # Therefore use current time for deletion.
    return datetime.now().timestamp()


# ============================================================
# STEP 5
# BACKDATED HOTELMANAGEMENT COMMITS IN CSE
# ============================================================


def commit_hotel_to_cse():
    print()
    print("==============================================")
    print("   HotelManagement → CSE GitHub")
    print("==============================================")
    print()

    changes = get_cse_hotel_changes()

    if not changes:
        print("ℹ️ No HotelManagement changes found inside CSE.")

        return True

    files_to_commit = []

    for status, filepath in changes:
        timestamp = get_file_timestamp(filepath)

        files_to_commit.append((status, filepath, timestamp))

    # Oldest modification first
    files_to_commit.sort(key=lambda x: x[2])

    print(f"Found {len(files_to_commit)} HotelManagement change(s).\n")

    successful_commits = 0

    # ========================================================
    # CREATE INDIVIDUAL COMMITS
    # ========================================================

    for status, filepath, timestamp in files_to_commit:
        date_str = datetime.fromtimestamp(timestamp).strftime("%Y-%m-%d %H:%M:%S")

        custom_env = os.environ.copy()

        custom_env["GIT_AUTHOR_DATE"] = date_str

        custom_env["GIT_COMMITTER_DATE"] = date_str

        filename = os.path.basename(filepath)

        # ----------------------------------------------------
        # Determine change type
        # ----------------------------------------------------

        if "D" in status:
            change_type = "Delete"

        elif "R" in status:
            change_type = "Rename"

        elif "?" in status:
            change_type = "Add"

        else:
            change_type = "Update"

        commit_message = f"{change_type} {filename}"

        print("----------------------------------------------")
        print(f"File   : {filepath}")
        print(f"Action : {change_type}")
        print(f"Date   : {date_str}")
        print(f"Commit : {commit_message}")

        # ----------------------------------------------------
        # Stage
        # ----------------------------------------------------

        try:
            if "D" in status:
                subprocess.run(
                    ["git", "rm", "--", filepath],
                    cwd=CSE_DIR,
                    check=True,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE,
                    text=True,
                )

            else:
                subprocess.run(
                    ["git", "add", "--", filepath],
                    cwd=CSE_DIR,
                    check=True,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE,
                    text=True,
                )

        except subprocess.CalledProcessError as e:
            print(f"⏩ Could not stage: {filepath}")

            if e.stderr:
                print(e.stderr)

            continue

        # ----------------------------------------------------
        # Commit
        # ----------------------------------------------------

        try:
            subprocess.run(
                ["git", "commit", "-m", commit_message],
                cwd=CSE_DIR,
                env=custom_env,
                check=True,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
            )

            print("✅ Commit created successfully.")

            successful_commits += 1

        except subprocess.CalledProcessError as e:
            print(f"⏩ Commit skipped: {filepath}")

            if e.stderr:
                print(e.stderr)

    # ========================================================
    # PUSH CSE
    # ========================================================

    if successful_commits == 0:
        print("\n❌ No CSE commits were created.")

        return False

    print()
    print("🚀 Pushing CSE repository to GitHub...")

    if run_git(["git", "push"], CSE_DIR):
        print("✅ CSE pushed successfully.")

        print(f"✅ {successful_commits} HotelManagement commit(s) created.")

        return True

    print("❌ CSE push failed.")

    return False


# ============================================================
# MAIN
# ============================================================


def main():
    print()
    print("==============================================")
    print("     HotelManagement Git Sync Tool")
    print("==============================================")
    print()

    # --------------------------------------------------------
    # 1. Efficiently sync HotelManagement → CSE
    # --------------------------------------------------------

    if not sync_hotel_to_cse():
        print("\n❌ Process stopped.")

        return

    # --------------------------------------------------------
    # 2. HotelManagement repo: commit + push
    # --------------------------------------------------------

    hotel_success = push_hotel_management()

    # --------------------------------------------------------
    # 3. CSE repo: backdated commit + push
    # --------------------------------------------------------

    cse_success = commit_hotel_to_cse()

    # --------------------------------------------------------
    # FINAL RESULT
    # --------------------------------------------------------

    print()
    print("==============================================")
    print("                  RESULT")
    print("==============================================")

    if hotel_success:
        print("✅ HotelManagement repository: DONE")

    else:
        print("❌ HotelManagement repository: FAILED")

    if cse_success:
        print("✅ CSE repository: DONE")

    else:
        print("❌ CSE repository: FAILED")

    print("==============================================")
    print()


# ============================================================
# PROGRAM ENTRY POINT
# ============================================================

if __name__ == "__main__":
    main()
