(startDate as date, endDate as date) as number =>
    let
        // Calculate total number of days between the two dates (exclusive of endDate)
        dayDiff = Duration.Days(endDate - startDate),

        // Generate a list of dates from startDate to (endDate - 1)
        datesList = List.Dates(startDate, dayDiff, #duration(1,0,0,0)),

        // Count only the weekdays (Monday to Friday) in the date list
        baseWorkingDays = List.Count(
            List.Select(
                datesList,
                each Date.DayOfWeek(_, Day.Monday) < 5 // DayOfWeek: 0=Monday, ..., 6=Sunday
            )
        ),

        // Check if endDate itself is a weekend (Saturday or Sunday)
        isWeekendEnd = Date.DayOfWeek(endDate, Day.Monday) >= 5,

        // If endDate is a weekend and there was at least one working day before it, subtract 1
        weekendOffset = if isWeekendEnd and baseWorkingDays > 0 then 1 else 0,

        // Final working days count, adjusting for weekend endDate
        workingDays = baseWorkingDays - weekendOffset
    in
        workingDays
