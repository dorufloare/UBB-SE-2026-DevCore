-- 1. THE RESET SWITCH: Safely drop and recreate the entire database
USE master;
GO

-- Force close any background connections to the database so we can delete it
ALTER DATABASE HospitalDatabase SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE HospitalDatabase;
GO

-- Create a fresh, empty database
CREATE DATABASE HospitalDatabase;
GO

USE HospitalDatabase;
GO

-- 2. CREATE TABLES
CREATE TABLE Staff (
    staff_id INT PRIMARY KEY IDENTITY(1,1),
    [role] VARCHAR(255),
    department VARCHAR(255),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    contact_info VARCHAR(255),
    is_available BIT,
    license_number VARCHAR(100),
    specialization VARCHAR(100),
    [status] VARCHAR(100),
    certification VARCHAR(100),
    years_of_experience INT
);

CREATE TABLE Shifts (
    shift_id INT PRIMARY KEY IDENTITY(1,1),
    staff_id INT NOT NULL,
    [location] VARCHAR(100),
    start_time DATETIME,
    end_time DATETIME,
    [status] VARCHAR(50),
    is_active BIT,
    CONSTRAINT FK_Shifts_Staff FOREIGN KEY (staff_id) REFERENCES Staff(staff_id)
);

CREATE TABLE Appointments (
    appointment_id INT PRIMARY KEY IDENTITY(1,1),
    patient_id INT NOT NULL, 
    doctor_id INT NOT NULL,
    start_time DATETIME,
    end_time DATETIME,
    [status] VARCHAR(50),
    CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (doctor_id) REFERENCES Staff(staff_id)
);

CREATE TABLE Medical_Evaluations (
    evaluation_id INT PRIMARY KEY IDENTITY(1,1),
    doctor_id INT NOT NULL,
    patient_id INT NOT NULL,
    diagnosis TEXT,
    doctor_notes TEXT,
    source VARCHAR(255),
    assumed_risk BIT,
    CONSTRAINT FK_Evaluations_Doctor FOREIGN KEY (doctor_id) REFERENCES Staff(staff_id)
);

CREATE TABLE Evaluation_Symptoms (
    evaluation_id INT NOT NULL,
    symptom_id INT NOT NULL,
    PRIMARY KEY (evaluation_id, symptom_id),
    CONSTRAINT FK_EvalSymp_Eval FOREIGN KEY (evaluation_id) REFERENCES Medical_Evaluations(evaluation_id)
);

CREATE TABLE Evaluation_Medications (
    evaluation_id INT NOT NULL,
    medication_id INT NOT NULL,
    PRIMARY KEY (evaluation_id, medication_id),
    CONSTRAINT FK_EvalMed_Eval FOREIGN KEY (evaluation_id) REFERENCES Medical_Evaluations(evaluation_id)
);

CREATE TABLE Hangouts (
    hangout_id INT PRIMARY KEY IDENTITY(1,1),
    title VARCHAR(25),
    description VARCHAR(100),
    date_time DATETIME,
    max_staff INT
);

CREATE TABLE Hangout_Participants (
    hangout_id INT NOT NULL,
    staff_id INT NOT NULL,
    PRIMARY KEY (hangout_id, staff_id),
    CONSTRAINT FK_HangoutPart_Hangout FOREIGN KEY (hangout_id) REFERENCES Hangouts(hangout_id),
    CONSTRAINT FK_HangoutPart_Staff FOREIGN KEY (staff_id) REFERENCES Staff(staff_id)
);

IF OBJECT_ID('High_Risk_Medicines', 'U') IS NOT NULL DROP TABLE High_Risk_Medicines;
CREATE TABLE High_Risk_Medicines (
    medicine_id INT PRIMARY KEY IDENTITY(1,1),
    medicine_name VARCHAR(100) NOT NULL,
    warning_message VARCHAR(255) NOT NULL
);
GO

-- 3. INSERT TEST DATA

-- Insert High Risk Medicines
INSERT INTO High_Risk_Medicines (medicine_name, warning_message)
VALUES 
('Warfarin', 'Blood thinner conflict: High risk of internal bleeding.'),
('Insulin', 'Glucose conflict: Requires immediate sugar level monitoring.'),
('Penicillin', 'Allergy Warning: History of anaphylaxis in this department.');

-- Insert Doctors (IDs 1 to 5)
INSERT INTO Staff ([role], department, first_name, last_name, is_available, specialization, status, contact_info, license_number, years_of_experience)
VALUES 
('Doctor', 'Cardiology', 'John', 'Smith', 1, 'Cardiologist', 'Available', 'info1', 'A1234', 20),
('Doctor', 'Emergency', 'Alice', 'Jones', 1, 'Surgeon', 'Available', 'info2', 'B4321', 15),
('Doctor', 'Diagnostic Medicine', 'Gregory', 'House', 1, 'Diagnostician', 'Available', 'info4', 'C9876', 25),
('Doctor', 'Oncology', 'James', 'Wilson', 1, 'Oncologist', 'Available', 'info5', 'D5555', 20),
('Doctor', 'Endocrinology', 'Lisa', 'Cuddy', 0, 'Endocrinologist', 'Off_Duty', 'info6', 'E7777', 18);

-- Insert Pharmacists (IDs 6 to 8)
INSERT INTO Staff ([role], department, first_name, last_name, is_available, status, contact_info, certification, years_of_experience)
VALUES 
('Pharmacist', 'Pharmacy', 'Robert', 'White', 0, 'Off_Duty', 'info3', 'BPS', 13),
('Pharmacist', 'Pharmacy', 'Jane', 'Doe', 1, 'Available', 'info7', 'PharmD', 8),
('Pharmacist', 'Pharmacy', 'Mark', 'Spencer', 1, 'Available', 'info8', 'BCPS', 5);

-- Insert Appointments
-- Recent/Current appointments
INSERT INTO Appointments (patient_id, doctor_id, start_time, [status])
VALUES (7759376, 1, GETDATE(), 'Confirmed');
INSERT INTO Appointments (patient_id, doctor_id, start_time, end_time, status)
VALUES (500, 1, '2026-04-05 10:30:00', '2026-04-05 11:30:00', 'Confirmed');
INSERT INTO Appointments (patient_id, doctor_id, start_time, end_time, status)
VALUES (501, 3, '2026-04-06 09:00:00', '2026-04-06 10:00:00', 'Scheduled');

-- FUTURE Appointments (For Hangout testing > 1 week away)
INSERT INTO Appointments (patient_id, doctor_id, start_time, end_time, status)
VALUES (502, 1, '2026-04-10 14:00:00', '2026-04-10 15:00:00', 'Scheduled'); -- Doc 1 is busy on April 10
INSERT INTO Appointments (patient_id, doctor_id, start_time, end_time, status)
VALUES (503, 2, '2026-04-15 10:00:00', '2026-04-15 11:00:00', 'Scheduled'); -- Doc 2 is busy on April 15

-- Insert Shifts (Mix of completed and scheduled for both Doctors and Pharmacists)
INSERT INTO Shifts (staff_id, location, start_time, end_time, status, is_active)
VALUES 
-- Doctor Shifts (Current/Recent)
(1, 'Cardiology Wing', '2026-04-01 08:00:00', '2026-04-01 16:00:00', 'Completed', 1),
(2, 'Ward A', '2026-04-01 08:00:00', '2026-04-01 16:00:00', 'Completed', 1),
(2, 'ER', '2026-04-02 14:00:00', '2026-04-02 22:00:00', 'Scheduled', 1),
(3, 'Clinic', '2026-04-02 09:00:00', '2026-04-02 17:00:00', 'Scheduled', 1),
(3, 'ICU', '2026-04-03 08:00:00', '2026-04-03 20:00:00', 'Scheduled', 1),
(4, 'Oncology Wing', '2026-04-01 09:00:00', '2026-04-01 17:00:00', 'Completed', 1),

-- FUTURE Doctor Shifts (For Hangout testing > 1 week away)
(1, 'Cardiology Wing', '2026-04-10 08:00:00', '2026-04-10 16:00:00', 'Scheduled', 1), -- Doc 1 shift on April 10
(2, 'ER', '2026-04-15 08:00:00', '2026-04-15 16:00:00', 'Scheduled', 1), -- Doc 2 shift on April 15
(3, 'Clinic', '2026-04-10 09:00:00', '2026-04-10 17:00:00', 'Scheduled', 1),

-- Pharmacist Shifts
(6, 'Main Pharmacy', '2026-04-01 08:00:00', '2026-04-01 16:00:00', 'Completed', 1),
(7, 'ER Pharmacy', '2026-04-02 16:00:00', '2026-04-03 00:00:00', 'Scheduled', 1),
(7, 'Main Pharmacy', '2026-04-03 08:00:00', '2026-04-03 16:00:00', 'Scheduled', 1),
(8, 'Main Pharmacy', '2026-04-01 16:00:00', '2026-04-02 00:00:00', 'Completed', 1);

-- Insert Medical Evaluations
INSERT INTO Medical_Evaluations (doctor_id, patient_id, diagnosis, doctor_notes, source, assumed_risk)
VALUES 
(1, 500, 'Mild Hypertension', 'Patient advised to reduce salt intake.', 'Physical Exam', 0),
(3, 501, 'Lupus Suspected', 'Ordering ANA panel and keeping patient under observation.', 'Lab Results', 1);

-- Insert Hangouts (Including future hangouts for testing)
INSERT INTO Hangouts (title, description, date_time, max_staff)
VALUES 
('Friday Pizza', 'Weekly team bonding in the breakroom', '2026-04-03 17:00:00', 10),
('Coffee Break', 'Quick catchup before morning rounds', '2026-04-02 07:30:00', 5),
('Future Movie Night', 'Watching a medical drama', '2026-04-10 19:00:00', 10), -- Test Hangout 1
('Mid-April Lunch', 'Lunch outing', '2026-04-15 12:30:00', 8); -- Test Hangout 2

-- Insert Hangout Participants
INSERT INTO Hangout_Participants (hangout_id, staff_id)
VALUES 
(1, 1), (1, 2), (1, 6), (1, 7), 
(2, 3), (2, 4);

-- Output validation
SELECT * FROM Staff;
SELECT * FROM Shifts;
GO
