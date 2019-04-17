import { Candidate } from "./Candidate";
import styles from './ContosoTalentWebPart.module.scss';
import { Recruiter } from "./Recruiter";
import { Location } from "./Location";

export class Position {
    public positionId: number;

    public positionExternalId: string;

    public title: string;

    public daysOpen: number;

    public description: string;

    public fullDescription: string;

    public candidates: Candidate[] = [];

    public hiringManager: Recruiter;

    public locationId: number;

    public location: Location;

    public hiringManagerId: number;

    public static GetJobInformationHeaderTemplate(jobInformation: Position, candidates: Candidate[]): string {
        let label: string = candidates.length === 1 ? 'candidate' : 'candidates';
        let hrStuff: Recruiter[] = Recruiter.GetHrStuff(jobInformation);
        let hiringTeamClass: string = hrStuff.length == 0 ? styles.hidden : "";
        let template = `
            <div class="${styles.jobTitleHeader}">
                <div class="${styles.row}">
                    <div class="ms-Grid-col ms-sm5">
                        <div class="${styles.row} ${styles.wideRow}"><strong class="ms-font-size-l">${jobInformation.title}</strong> (${candidates.length} ${label})</div>
                    </div>
                    <div class="ms-Grid-col ms-sm4">
                        <div class="${styles.row} ${styles.wideRow}"><span>Hiring Manager: </span><strong>${jobInformation.hiringManager.name}</strong></div></div>
                    <div class="ms-Grid-col ms-sm3">
                         <div class="${styles.row} ${styles.wideRow}"><span>Location: </span><strong>${jobInformation.location.city}, ${jobInformation.location.state}</strong></div>
                    </div>
                </div>
                <div class="${styles.row}" style="padding: 0 20px 20px;">
                    <div class="ms-Grid-col ms-sm5">
                        <div class="${styles.row} ${styles.collapsible}" data-id="${jobInformation.positionId}">
                            Job Details 
                            <i class="ms-Icon ms-Icon--ChevronDown ${styles.iconRight} details-${jobInformation.positionId}" title="Open" aria-hidden="true" data-id="${jobInformation.positionId}"></i>
                            <i class="ms-Icon ms-Icon--ChevronUp ${styles.iconRight} ${styles.hidden} details-${jobInformation.positionId}" title="Close" aria-hidden="true" data-id="${jobInformation.positionId}"></i>
                        </div>
                        <div class="${styles.collapsibleContent} cm-data-${jobInformation.positionId}" style="padding-left: 12px;" data-id="${jobInformation.positionId}">
                            <div class="${styles.internalRow}" style="margin: 0;" title="${jobInformation.fullDescription}">${jobInformation.description}</div>
                        </div>
                    </div>
                    <div class="ms-Grid-col ms-sm3 ${hiringTeamClass}">
                        <div class="${styles.row} ${styles.collapsible}" data-id="${jobInformation.positionId}">
                            Hiring Team
                            <i class="ms-Icon ms-Icon--ChevronDown ${styles.iconRight} details-${jobInformation.positionId}" title="Open" aria-hidden="true" data-id="${jobInformation.positionId}"></i>
                            <i class="ms-Icon ms-Icon--ChevronUp ${styles.iconRight} ${styles.hidden} details-${jobInformation.positionId}" title="Close" aria-hidden="true" data-id="${jobInformation.positionId}"></i>
                        </div>
                        <div class="${styles.collapsibleContent} cm-data-${jobInformation.positionId}" data-id="${jobInformation.positionId}">
                        <div class="${styles.internalRow}">${Recruiter.GetHrTemplate(hrStuff)}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        return template;
    }

    public static BuildJobsInformation(candidates: Candidate[], allPositions: Position[]): Position[] {
        let availableJobsInformation: Position[] = [];

        candidates.forEach(c => {
            let currentPosition: Position = allPositions.filter(p => p.positionId === c.positionId)[0];
            if(availableJobsInformation.filter(p => p.positionId === currentPosition.positionId).length === 0){
                availableJobsInformation.push(currentPosition);
            }
        });

        return availableJobsInformation;
    }
}
