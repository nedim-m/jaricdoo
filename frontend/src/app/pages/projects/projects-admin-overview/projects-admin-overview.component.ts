import { Component } from '@angular/core';
import { ProjectsService } from '../projects.service';
import { ProjectResponse } from '../projects.model';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-projects-admin-overview',
  standalone: true,
  imports: [CommonModule, TableModule,FormsModule],
  templateUrl: './projects-admin-overview.component.html',
  styleUrl: './projects-admin-overview.component.css',
})
export class ProjectsAdminOverviewComponent {
  projects: ProjectResponse[] = [];
  searchLocation: string = '';

  constructor(private projectsService: ProjectsService, private router: Router) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  navigateToEdit(projectId: number) {
    this.router.navigate(['/projects/edit', projectId]);
  }

  loadProjects(): void {
    this.projectsService.getAllProjects().subscribe({
      next: (data) => {
        this.projects = data.result;
      },
      error: (err) => {
        console.error('Greška prilikom preuzimanja projekata:', err);
      },
    });
  }

  onSearchChange() {
    if (this.searchLocation) {
      this.projects = this.projects.filter(project =>
        project.location.toLowerCase().includes(this.searchLocation.toLowerCase())
      );
    } else {
      this.loadProjects(); 
    }
  }

 

  onDelete(projectId: number): void {
    Swal.fire({
      title: 'Jeste li sigurni?',
      text: 'Ovaj projekat i sve njegove slike bit će trajno obrisani.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Da, obriši!',
      cancelButtonText: 'Otkaži'
    }).then((result) => {
      if (result.isConfirmed) {
        
        this.projectsService.deleteProject(projectId).subscribe(() => {
          Swal.fire(
            'Obrisano!',
            'Projekat je uspješno obrisan.',
            'success'
          );
         
          this.loadProjects();
        }, error => {
          Swal.fire(
            'Greška!',
            'Došlo je do problema prilikom brisanja projekta.',
            'error'
          );
        });
      }
    });
  }
  navigateToAddProject() {
    this.router.navigate(['insert-projects']);
  }
}
