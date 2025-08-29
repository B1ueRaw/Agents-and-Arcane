import sqlite3

def init_sqlite(filename):
    con = sqlite3.connect(filename, check_same_thread=False)
    cur = con.cursor()
    cur.execute("DROP TABLE IF EXISTS Units;")
    cur.execute("DROP TABLE IF EXISTS Equipment;")
    cur.execute("CREATE TABLE Units(unit_id TEXT PRIMARY KEY, x_coord INT NOT NULL, y_coord INT NOT NULL, thread_id TEXT, hp UNSIGNED INT NOT NULL, class_id INT NOT NULL, equip_id INT, team BOOL, has_moved BOOL, orders TEXT, FOREIGN KEY(equip_id) REFERENCES Equipment(equip_id), FOREIGN KEY(class_id) REFERENCES Classes(class_id));")
    cur.execute("CREATE TABLE Equipment(equip_id INT PRIMARY KEY, tier INT NOT NULL, description TEXT);")
    
    cur.execute("CREATE TABLE IF NOT EXISTS Resources(x_coord INT NOT NULL, y_coord INT NOT NULL, type INT NOT NULL);")
    cur.execute("CREATE TABLE IF NOT EXISTS Classes(class_id INT PRIMARY KEY, base_hp INT, base_ms INT, ability TEXT);")
    con.commit()
    return cur.execute, con.commit