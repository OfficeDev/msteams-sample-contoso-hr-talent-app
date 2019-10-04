import { Location } from "./Location";
import { Position } from "./Position";
import { Interview } from "./Interview";
import { IconHelper } from "./utilities/IconHelper";
import styles from './ContosoTalentWebPart.module.scss';

export class Candidate {
    public candidateId: number;

    public name: string;

    public positionTitle: string;

    public stage: string;

    public phone: string;

    public currentRole: string;

    public profilePicture: string;

    public positionId: number;

    public locationId: number;

    public location: Location;

    public interviews: Interview[];

    public static GetFullGridTemplate(jobsInformation: Position[], candidates: Candidate[], offlineMode: boolean): string {
        let result = ``;
        jobsInformation.forEach((ji: Position) => {
            let applicableCandidates: Candidate[] = candidates.filter((c: Candidate) => c.positionId === ji.positionId);
            result +=
              ` ${Position.GetJobInformationHeaderTemplate(ji, applicableCandidates)}
                ${this.GetCandidateGrid(applicableCandidates, offlineMode)}
            `;
          });

        return result;
    }

    public static GetCandidateGrid(candidates: Candidate[], offlineMode: boolean): string {
        let candidatesRows = ``;
        candidates.forEach((c: Candidate, index: number) => {
            candidatesRows += `
                <div class="ms-Grid-row ${styles.itemRow}">
                    <div class="ms-sm1 ms-Grid-col">${IconHelper.BuildIconTemplate(c, false, true)}</div>
                    <div class="ms-sm4 ms-Grid-col">${c.name}</div>
                    <div class="ms-sm2 ms-Grid-col">${c.phone}</div>
                    <div class="ms-sm3 ms-Grid-col">${c.location.city}, ${c.location.state}</div>
                    <div class="ms-sm2 ms-Grid-col stage-btn ${styles.dropdown}" data-index="${index}" data-positionId="${c.positionId}">
                        <div class="ms-sm12 ${styles.hideWideContent}">
                            <span class="${offlineMode ? `` : styles.stageColumn} label-${c.candidateId}" data-index="${index}" data-positionId="${c.positionId}">${c.stage}</span>
                            ${this.getChevronMenu(offlineMode, index, c.positionId)}
                        </div>
                        <div id="stagesDropdown-${c.positionId}-${index}" class="${styles.dropdownContent}">
                            <a class="change-state-btn" data-index="${index}" data-id="${c.candidateId}" data-positionId="${c.positionId}" data-stage="Applied" href="#applied" style="display: ${c.stage === 'Applied' ? 'none;' : 'block'}">Applied</a>
                            <a class="change-state-btn" data-index="${index}" data-id="${c.candidateId}" data-positionId="${c.positionId}" data-stage="Screening" href="#screening" style="display: ${c.stage === 'Screening' ? 'none;' : 'block'}">Screening</a>
                            <a class="change-state-btn" data-index="${index}" data-id="${c.candidateId}" data-positionId="${c.positionId}" data-stage="Interviewing" href="#interviewing" style="display: ${c.stage === 'Interviewing' ? 'none;' : 'block'}">Interviewing</a>
                            <a class="change-state-btn" data-index="${index}" data-id="${c.candidateId}" data-positionId="${c.positionId}" data-stage="Offered" href="#offer" style="display: ${c.stage === 'Offered' ? 'none;' : 'block'}">Offer</a>
                        </div>
                    </div>
                </div>
            `;
        });
        
        let template: string = `
            <div class="${styles.row} ${styles.wideRow}">
                <div class="ms-Grid ${styles.gridList}">
                    <div class="ms-Grid-row ${styles.headerRow}">
                        <div class="ms-sm1 ms-Grid-col"></div>
                        <div class="ms-sm4 ms-Grid-col">Name</div>
                        <div class="ms-sm2 ms-Grid-col">Phone</div>
                        <div class="ms-sm3 ms-Grid-col">Location</div>
                        <div class="ms-sm2 ms-Grid-col">Current Stage</div>
                    </div>
                    ${candidatesRows}
                </div>
            </div>
        `;

        return template;
    }

    public static getChevronMenu(offlineMode: boolean, index: number, positionId: number): string {
        let chevronMenu: string = offlineMode ? "" : `<i data-positionId="${positionId}" data-index="${index}" class="ms-Icon ms-Icon--ChevronDown ${styles.icon}" title="Open" aria-hidden="true"></i>`;
        return chevronMenu;
    }
}
