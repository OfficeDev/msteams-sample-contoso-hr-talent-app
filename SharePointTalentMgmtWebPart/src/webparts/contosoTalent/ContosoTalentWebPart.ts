import { Version } from '@microsoft/sp-core-library';
import {
  BaseClientSideWebPart,
  IPropertyPaneConfiguration,
  PropertyPaneTextField
} from '@microsoft/sp-webpart-base';

import styles from './ContosoTalentWebPart.module.scss';
import * as strings from 'ContosoTalentWebPartStrings';

import { Candidate } from './Candidate';
import { Position } from './Position';
import { Recruiter } from './Recruiter';
import { Location } from './Location';
import { ApiService } from './utilities/Api.service';
import { HeaderBuilder } from './utilities/HeaderBuilder';
import { EventHelper } from './utilities/EventHelper';
import { Interview } from './Interview';

let candidatesJson: any = require('./candidates.json');
let settings: any = require('./settings.json');
let positionsJson: any = require('./positions.json');
let recruitersJson: any = require('./recruiters.json');
let interviewsJson: any = require('./interviews.json');
let locationsJson: any = require('./locations.json');

export interface IContosoTalentWebPartProps {
  description: string;
}

const ALL_FILTER_TYPE: string = "All";
const SERVICE_URL_KEY: string = "serviceUrl";

export default class ContosoTalentWebPartProps extends BaseClientSideWebPart<IContosoTalentWebPartProps> {
  private _teamsContext: microsoftTeams.Context;
  private offlineMode: boolean;
  private cachedPositions: Position[] = [];
  private allCandidates: Candidate[] = [];
  private stageFilter: string = ALL_FILTER_TYPE;
  private positionsFilter: string = ALL_FILTER_TYPE;
  private connectToServiceUrl: string = "";

  protected onInit(): Promise<any> {
    let retVal: Promise<any> = Promise.resolve();
    if (this.context.microsoftTeams) {
      retVal = new Promise((resolve, reject) => {
        this.context.microsoftTeams.getContext(context => {
          this._teamsContext = context;
          resolve();
        });
      });
    }
    return retVal;
  }

  public render(): void {
    this.buildWebPart();
  }

  private buildWebPart(): void {
    if(this.properties != null && this.properties[SERVICE_URL_KEY] != null && this.properties[SERVICE_URL_KEY].length > 0){
      this.connectToServiceUrl = this.properties[SERVICE_URL_KEY];
    }
    else {
      this.connectToServiceUrl = settings.ServiceUrl;
    }

    let positions: Position[] = this.Positions;
    this.getCandidates(positions);
    let baseClass: string = this._teamsContext == null ? styles.sharepoint : styles.teams;

    let filteredCandidates: Candidate[] = this.stageFilter === ALL_FILTER_TYPE ? this.allCandidates : this.allCandidates.filter((c: Candidate) => c.stage === this.stageFilter);
    let jobsInformation: Position[] = Position.BuildJobsInformation(filteredCandidates, positions);
    let filteredPositions: Position[] = this.positionsFilter === ALL_FILTER_TYPE ? jobsInformation : jobsInformation.filter((j: Position) => j.title === this.positionsFilter);
    this.domElement.innerHTML =
      `
        <div class="${baseClass}">
          ${HeaderBuilder.BuildHeader(this.allCandidates, this.stageFilter, this.offlineMode)}
          ${HeaderBuilder.BuildPositionSelectorTemplate(this.positionsFilter, jobsInformation)}
          ${Candidate.GetFullGridTemplate(filteredPositions, filteredCandidates, this.offlineMode)}
        </div>
    `;

    this.bindEvents();
  }

  private get Positions(): Position[] {
    if (this.cachedPositions != null && this.cachedPositions.length > 0) {
      return this.cachedPositions;
    }

    let positions: Position[] = [];
    try {
      let candidatesAsJson: string = ApiService.GetServerData(this.connectToServiceUrl + '/api/positions');
      positions = JSON.parse(candidatesAsJson);
    }
    catch (error) {
      positions = this.populateOfflineData();
    }

    this.cachedPositions = positions;
    return positions;
  }

  private getCandidates(positions: Position[]): void {
    positions.forEach((p: Position) => {
      p.candidates.forEach((c: Candidate) => {
        if(this.allCandidates.filter(ac => ac.candidateId === c.candidateId).length === 0){
          this.allCandidates.push(c);
        }
      });
    });
  }

  private bindEvents(): void {
    EventHelper.RegisterGlobalEventHandlers();
    EventHelper.AddClickEventListener(styles.block, (element: Element) => {
      this.stageFilter = element.getAttribute("data-type");
      this.positionsFilter = ALL_FILTER_TYPE;
      this.buildWebPart();
    });

    if(!this.offlineMode){
      EventHelper.AddClickEventListener("stage-btn", (element: Element) => {
        EventHelper.OpenCloseDropdown(`stagesDropdown-${element.getAttribute("data-positionId")}-${element.getAttribute("data-index")}`);
      }, true);
    
      EventHelper.AddClickEventListener("change-state-btn", (element: Element) => {
        this.updateCandidateStage(element);
      }, true);
    }

    EventHelper.AddClickEventListener("change-position-btn", (element: Element) => {
      this.positionsFilter = element.getAttribute("data-position");
      this.buildWebPart();
    }, true);
  }

  private updateCandidateStage(element: Element): void {
    let stage: string = element.getAttribute("data-stage");
    let candidateId: string = element.getAttribute("data-id");
    (<HTMLElement>document.getElementsByClassName("label-" + candidateId)[0]).innerText = stage;
    let candidateToUpdate: Candidate =  this.allCandidates.filter(c => c.candidateId === parseInt(candidateId))[0];
    const oldCandidateStage: string = candidateToUpdate.stage;
    candidateToUpdate.stage = stage;

    if(!this.offlineMode){
      ApiService.UpdateServerData(this.connectToServiceUrl + '/api/candidates/', { candidateId: candidateId, stage: stage});
    }

    if (this.allCandidates.filter((c => c.stage === oldCandidateStage)).length === 0) { this.stageFilter = ALL_FILTER_TYPE; this.positionsFilter = ALL_FILTER_TYPE; }
    EventHelper.CloseAllDropdowns();
    this.buildWebPart();
  }

  private populateOfflineData(): Position[] {
      var positions = <Position[]>positionsJson;
      var candidates = <Candidate[]>candidatesJson;
      var recruiters = <Recruiter[]>recruitersJson;
      var interviews = <Interview[]>interviewsJson;
      var locations = <Location[]>locationsJson;
      positions.forEach(p => {
        p.candidates = [];
        p.hiringManager = recruiters.filter(r => r.recruiterId === p.hiringManagerId)[0];
        p.location = locations.filter(l => l.locationId === p.locationId)[0];
        let candidatesForThisPosition: Candidate[] = candidates.filter(c => c.positionId === p.positionId);
        candidatesForThisPosition.forEach((c: Candidate) => {
          c.interviews = interviews.filter((i: Interview) => i.candidateId === c.candidateId);
          if(c.interviews && c.interviews.length > 0) {
            c.interviews.forEach(ci => {
              ci.recruiter = recruiters.filter(r => r.recruiterId === ci.recruiterId)[0];
            });
          }
          c.location = locations.filter(l => l.locationId === c.locationId)[0];
          p.candidates.push(c);
        });
      });
      this.offlineMode = true;

      return positions;
  }

  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          groups: [
            {
              groupName: 'Configure your webpart',
              groupFields: [
                PropertyPaneTextField(SERVICE_URL_KEY, {
                  label: "Service URL:",
                  placeholder : settings.ServiceUrl
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
