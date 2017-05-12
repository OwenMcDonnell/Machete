declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-' + convert(varchar(5), min(WW.gender)) as id,
L.text_EN as label, 
count(*) as value
FROM (
  select W.ID, W.gender
  from persons W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.gender
) as WW
JOIN dbo.Lookups L ON L.ID = WW.gender
group by L.text_EN

union 

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-NULL' as id,
'NULL' as label, 
count(*) as value
from (
   select W.ID, min(dateforsignin) firstsignin
   from persons W
   JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
   WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
   and W.gender is null
   group by W.ID
) as WWW
